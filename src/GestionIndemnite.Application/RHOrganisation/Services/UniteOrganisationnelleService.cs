using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services;

public class UniteOrganisationnelleService
{
    private readonly IRHOrganisationDbContext _db;

    public UniteOrganisationnelleService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<UniteOrganisationnelleDto> CreateAsync(
        CreateUniteOrganisationnelleCommand cmd,
        CancellationToken ct = default)
    {
        var nom = (cmd.NomUnite ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(nom))
            throw new RegleMetierException("Le nom de l'unité organisationnelle est obligatoire.");

        if (cmd.IdUniteParent.HasValue)
        {
            var exists = await _db.UnitesOrganisationnelles
                .AnyAsync(u => u.Id == cmd.IdUniteParent.Value, ct);
            if (!exists)
                throw new RegleMetierException("L'unité organisationnelle parente est introuvable.");
        }

        var dateDebut = DateOnly.FromDateTime(DateTime.UtcNow);
        if (await IsCycleAsync(cmd.IdUniteParent, dateDebut, ct))
            throw new RegleMetierException("Incohérence hiérarchique : une unité ne peut pas être rattachée à elle-même ni à l'une de ses unités subordonnées.");

        var unite = new UniteOrganisationnelle
        {
            Id = Guid.NewGuid(),
            NomUnite = nom,
            NomUniteAr = (cmd.NomUniteAr ?? string.Empty).Trim(),
            TypeUnite = cmd.TypeUnite,
            IdVille = cmd.IdVille,
            DateCreation = cmd.DateCreation ?? DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        };

        _db.UnitesOrganisationnelles.Add(unite);

        _db.HistoriqueOrganigrammeUniteSet.Add(new HistoriqueOrganigrammeUnite
        {
            Id = Guid.NewGuid(),
            IdUnite = unite.Id,
            IdUniteParent = cmd.IdUniteParent,
            DateDebut = dateDebut,
            DateFin = null
        });

        await _db.SaveChangesAsync(ct);

        return ToDto(unite);
    }

    public async Task<UniteOrganisationnelleDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var unite = await _db.UnitesOrganisationnelles.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new RegleMetierException("L'unité organisationnelle est introuvable.");
        return ToDto(unite);
    }

    public async Task<IReadOnlyList<UniteOrganisationnelleDto>> ListAsync(
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var query = _db.UnitesOrganisationnelles.AsNoTracking();
        if (!includeInactive)
            query = query.Where(u => u.IsActive);

        var list = await query.OrderBy(u => u.NomUnite).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<ParentOrganigrammeDto?> GetParentAtAsync(
        Guid idUnite,
        DateOnly? date = null,
        CancellationToken ct = default)
    {
        var at = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var record = await _db.HistoriqueOrganigrammeUniteSet.AsNoTracking()
            .Where(h => h.IdUnite == idUnite
                && h.DateDebut <= at
                && (h.DateFin == null || h.DateFin >= at))
            .OrderByDescending(h => h.DateDebut)
            .FirstOrDefaultAsync(ct);

        if (record?.IdUniteParent is null)
            return null;

        var parent = await _db.UnitesOrganisationnelles.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == record.IdUniteParent, ct)
            ?? throw new RegleMetierException("L'unité organisationnelle parente est introuvable.");

        return new ParentOrganigrammeDto(parent.Id, parent.NomUnite, record.DateDebut, record.DateFin);
    }

    public async Task DisableAsync(Guid id, CancellationToken ct = default)
    {
        var unite = await _db.UnitesOrganisationnelles.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new RegleMetierException("L'unité organisationnelle est introuvable.");
        unite.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var unite = await _db.UnitesOrganisationnelles.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new RegleMetierException("L'unité organisationnelle est introuvable.");
        unite.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<bool> IsCycleAsync(Guid? parentId, DateOnly date, CancellationToken ct)
    {
        if (parentId is null)
            return false;

        var visited = new HashSet<Guid>();
        var current = parentId;
        var guard = 0;

        while (current is not null && guard++ < 100)
        {
            if (!visited.Add(current.Value))
                return true;

            var record = await _db.HistoriqueOrganigrammeUniteSet.AsNoTracking()
                .Where(h => h.IdUnite == current.Value
                    && h.DateDebut <= date
                    && (h.DateFin == null || h.DateFin >= date))
                .OrderByDescending(h => h.DateDebut)
                .FirstOrDefaultAsync(ct);

            current = record?.IdUniteParent;
        }

        return false;
    }

    public async Task<UniteOrganisationnelleDto> UpdateAsync(
        Guid id,
        UpdateUniteOrganisationnelleCommand cmd,
        CancellationToken ct = default)
    {
        var unite = await _db.UnitesOrganisationnelles.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new RegleMetierException("L'unité organisationnelle est introuvable.");

        var nom = (cmd.NomUnite ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(nom))
            throw new RegleMetierException("Le nom de l'unité organisationnelle est obligatoire.");

        unite.NomUnite = nom;
        unite.NomUniteAr = (cmd.NomUniteAr ?? string.Empty).Trim();
        unite.TypeUnite = cmd.TypeUnite;
        unite.IdVille = cmd.IdVille;
        unite.DateCreation = cmd.DateCreation ?? unite.DateCreation;

        await _db.SaveChangesAsync(ct);
        return ToDto(unite);
    }

    private static UniteOrganisationnelleDto ToDto(UniteOrganisationnelle u) =>
        new(u.Id, u.NomUnite, u.TypeUnite, u.IdVille, u.IsActive, u.NomUniteAr)
        {
            DateCreation = u.DateCreation
        };
}
