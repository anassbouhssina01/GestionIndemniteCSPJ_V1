using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Domain.RHOrganisation.Rules;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services;

public class TribunalService
{
    private readonly IRHOrganisationDbContext _db;

    public TribunalService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<TribunalDto> CreateAsync(
        CreateTribunalCommand cmd,
        CancellationToken ct = default)
    {
        var nom = (cmd.NomTribunal ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(nom))
            throw new RegleMetierException("Le nom du tribunal est obligatoire.");

        var numero = (cmd.NumeroTribunal ?? string.Empty).Trim();
        if (numero.Length > 50)
            throw new RegleMetierException("Le numéro du tribunal ne peut pas dépasser 50 caractères.");
        if (!string.IsNullOrEmpty(numero) &&
            await _db.Tribunaux.AnyAsync(t => t.NumeroTribunal == numero, ct))
            throw new RegleMetierException($"Le numéro de tribunal «{numero}» est déjà utilisé.");

        NiveauJuridiction? parentLevel = null;
        if (cmd.IdTribunalParent.HasValue)
        {
            var parent = await _db.Tribunaux
                .FirstOrDefaultAsync(t => t.Id == cmd.IdTribunalParent.Value, ct)
                ?? throw new RegleMetierException("Le tribunal parent est introuvable.");
            parentLevel = parent.NiveauJuridiction;
        }

        OrganigrammeRules.ValidateTribunalParent(cmd.NiveauJuridiction, parentLevel);

        var dateDebut = DateOnly.FromDateTime(DateTime.UtcNow);
        if (await IsCycleAsync(cmd.IdTribunalParent, dateDebut, ct))
            throw new RegleMetierException("Incohérence hiérarchique : un tribunal ne peut pas être rattaché à lui-même ni à l'un de ses tribunaux subordonnés.");

        var tribunal = new Tribunal
        {
            Id = Guid.NewGuid(),
            NumeroTribunal = numero,
            NomTribunal = nom,
            NomTribunalAr = (cmd.NomTribunalAr ?? string.Empty).Trim(),
            TypeTribunal = cmd.TypeTribunal,
            NiveauJuridiction = cmd.NiveauJuridiction,
            IdVille = cmd.IdVille,
            DateCreation = cmd.DateCreation ?? DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        };

        _db.Tribunaux.Add(tribunal);

        _db.HistoriqueOrganigrammeTribunalSet.Add(new HistoriqueOrganigrammeTribunal
        {
            Id = Guid.NewGuid(),
            IdTribunal = tribunal.Id,
            IdTribunalParent = cmd.IdTribunalParent,
            DateDebut = dateDebut,
            DateFin = null
        });

        await _db.SaveChangesAsync(ct);

        return ToDto(tribunal);
    }

    public async Task<TribunalDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var tribunal = await _db.Tribunaux.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new RegleMetierException("Le tribunal est introuvable.");
        return ToDto(tribunal);
    }

    public async Task<IReadOnlyList<TribunalDto>> ListAsync(
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var query = _db.Tribunaux.AsNoTracking();
        if (!includeInactive)
            query = query.Where(t => t.IsActive);

        var list = await query.OrderBy(t => t.NomTribunal).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<ParentOrganigrammeDto?> GetParentAtAsync(
        Guid idTribunal,
        DateOnly? date = null,
        CancellationToken ct = default)
    {
        var at = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var record = await _db.HistoriqueOrganigrammeTribunalSet.AsNoTracking()
            .Where(h => h.IdTribunal == idTribunal
                && h.DateDebut <= at
                && (h.DateFin == null || h.DateFin >= at))
            .OrderByDescending(h => h.DateDebut)
            .FirstOrDefaultAsync(ct);

        if (record?.IdTribunalParent is null)
            return null;

        var parent = await _db.Tribunaux.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == record.IdTribunalParent, ct)
            ?? throw new RegleMetierException("Le tribunal parent est introuvable.");

        return new ParentOrganigrammeDto(parent.Id, parent.NomTribunal, record.DateDebut, record.DateFin);
    }

    public async Task DisableAsync(Guid id, CancellationToken ct = default)
    {
        var tribunal = await _db.Tribunaux.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new RegleMetierException("Le tribunal est introuvable.");
        tribunal.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var tribunal = await _db.Tribunaux.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new RegleMetierException("Le tribunal est introuvable.");
        tribunal.IsActive = true;
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

            var record = await _db.HistoriqueOrganigrammeTribunalSet.AsNoTracking()
                .Where(h => h.IdTribunal == current.Value
                    && h.DateDebut <= date
                    && (h.DateFin == null || h.DateFin >= date))
                .OrderByDescending(h => h.DateDebut)
                .FirstOrDefaultAsync(ct);

            current = record?.IdTribunalParent;
        }

        return false;
    }

    public async Task<TribunalDto> UpdateAsync(
        Guid id,
        UpdateTribunalCommand cmd,
        CancellationToken ct = default)
    {
        var tribunal = await _db.Tribunaux.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new RegleMetierException("Le tribunal est introuvable.");

        var nom = (cmd.NomTribunal ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(nom))
            throw new RegleMetierException("Le nom du tribunal est obligatoire.");

        var numero = (cmd.NumeroTribunal ?? string.Empty).Trim();
        if (numero.Length > 50)
            throw new RegleMetierException("Le numéro du tribunal ne peut pas dépasser 50 caractères.");
        if (!string.IsNullOrEmpty(numero) &&
            await _db.Tribunaux.AnyAsync(t => t.NumeroTribunal == numero && t.Id != id, ct))
            throw new RegleMetierException($"Le numéro de tribunal «{numero}» est déjà utilisé.");

        tribunal.NumeroTribunal = numero;
        tribunal.NomTribunal = nom;
        tribunal.NomTribunalAr = (cmd.NomTribunalAr ?? string.Empty).Trim();
        tribunal.TypeTribunal = cmd.TypeTribunal;
        tribunal.NiveauJuridiction = cmd.NiveauJuridiction;
        tribunal.IdVille = cmd.IdVille;
        tribunal.DateCreation = cmd.DateCreation ?? tribunal.DateCreation;

        await _db.SaveChangesAsync(ct);
        return ToDto(tribunal);
    }

    private static TribunalDto ToDto(Tribunal t) =>
        new(t.Id, t.NomTribunal, t.TypeTribunal, t.NiveauJuridiction, t.IdVille, t.IsActive,
            t.NomTribunalAr)
        {
            NumeroTribunal = t.NumeroTribunal,
            DateCreation = t.DateCreation
        };
}
