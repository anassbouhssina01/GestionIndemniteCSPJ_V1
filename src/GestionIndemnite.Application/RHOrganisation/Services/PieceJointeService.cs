using System.Security.Cryptography;
using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Domain.RHOrganisation.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionIndemnite.Application.RHOrganisation.Services;

public class PieceJointeService
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private readonly IRHOrganisationDbContext _db;
    private readonly IFileStorage _storage;
    private readonly ILogger<PieceJointeService> _logger;

    public PieceJointeService(
        IRHOrganisationDbContext db,
        IFileStorage storage,
        ILogger<PieceJointeService> logger)
    {
        _db = db;
        _storage = storage;
        _logger = logger;
    }

    public async Task<PieceJointeDto> UploadAsync(
        Stream fileStream,
        UploadPieceJointeCommand cmd,
        CancellationToken ct = default)
    {
        if (fileStream.Length > MaxFileSize)
            throw new RegleMetierException("حجم الملف يتجاوز 10 ميغا");

        var header = new byte[4];
        var read = await fileStream.ReadAsync(header.AsMemory(0, 4), ct);
        if (read < 4 || header[0] != (byte)'%' || header[1] != (byte)'P'
            || header[2] != (byte)'D' || header[3] != (byte)'F')
            throw new RegleMetierException("الملف ليس بصيغة PDF صحيحة");

        fileStream.Position = 0;

        string hash;
        using (var sha = SHA256.Create())
        {
            var hashBytes = await sha.ComputeHashAsync(fileStream, ct);
            hash = Convert.ToHexString(hashBytes);
            fileStream.Position = 0;
        }

        var numeroNormalise = TextNormalizer.Normalize(cmd.NumeroDocument);
        var emetteurNormalise = TextNormalizer.Normalize(cmd.Emetteur);

        var existing = await _db.PieceJointes.FirstOrDefaultAsync(
            p => p.NumeroDocumentNormalise == numeroNormalise
              && p.AnneeDocument == cmd.DateDocument.Year
              && p.EmetteurNormalise == emetteurNormalise,
            ct);

        if (existing is not null)
        {
            throw new RegleMetierException(
                $"المستند موجود مسبقاً: {existing.NumeroDocument} — {existing.Emetteur} ({existing.AnneeDocument})");
        }

        var storedName = await _storage.SaveAsync(fileStream, $"{numeroNormalise}.pdf", ct);

        var entity = new PieceJointe
        {
            Id = Guid.NewGuid(),
            NumeroDocument = cmd.NumeroDocument.Trim(),
            NumeroDocumentNormalise = numeroNormalise,
            DateDocument = cmd.DateDocument,
            AnneeDocument = cmd.DateDocument.Year,
            Emetteur = cmd.Emetteur.Trim(),
            EmetteurNormalise = emetteurNormalise,
            TypeDocument = cmd.TypeDocument.Trim(),
            Objet = cmd.Objet.Trim(),
            NomFichier = $"{cmd.NumeroDocument.Trim()}.pdf",
            CheminFichier = storedName,
            HashFichier = hash,
            DateAjout = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _db.PieceJointes.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            _storage.Delete(storedName);
            _logger.LogError("Échec d'enregistrement du document, fichier supprimé: {StoredName}", storedName);
            throw;
        }

        return ToDto(entity);
    }

    public async Task<Stream> DownloadAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PieceJointes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegleMetierException("المستند غير موجود");

        if (entity.CheminFichier is null)
            throw new RegleMetierException("ملف هذا المستند غير متوفر");

        return await _storage.OpenReadAsync(entity.CheminFichier, ct);
    }

    public async Task<PieceJointeDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PieceJointes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegleMetierException("المستند غير موجود");
        return ToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PieceJointes.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegleMetierException("المستند غير موجود");

        var utilise = await _db.HistoriqueCadreGrades.AnyAsync(h => h.IdDocument == id, ct)
            || await _db.SituationAdministratives.AnyAsync(h => h.IdDocument == id, ct)
            || await _db.HistoriqueAffectations.AnyAsync(h => h.IdDocument == id, ct)
            || await _db.HistoriqueResponsabilites.AnyAsync(h => h.IdDocument == id, ct)
            || await _db.HistoriqueOrganigrammeTribunalSet.AnyAsync(h => h.IdDocument == id, ct)
            || await _db.HistoriqueOrganigrammeUniteSet.AnyAsync(h => h.IdDocument == id, ct);
        if (utilise)
            throw new RegleMetierException("لا يمكن حذف مستند مرتبط بسجل");

        _db.PieceJointes.Remove(entity);
        await _db.SaveChangesAsync(ct);

        if (entity.CheminFichier is not null)
            _storage.Delete(entity.CheminFichier);
    }

    public async Task<IReadOnlyList<PieceJointeDto>> SearchAsync(
        string? numeroDocument,
        string? emetteur,
        int? annee,
        CancellationToken ct = default)
    {
        var query = _db.PieceJointes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(numeroDocument))
        {
            var n = TextNormalizer.Normalize(numeroDocument);
            query = query.Where(p => p.NumeroDocumentNormalise.Contains(n));
        }

        if (!string.IsNullOrWhiteSpace(emetteur))
        {
            var e = TextNormalizer.Normalize(emetteur);
            query = query.Where(p => p.EmetteurNormalise.Contains(e));
        }

        if (annee.HasValue)
            query = query.Where(p => p.AnneeDocument == annee);

        var list = await query.OrderByDescending(p => p.DateDocument).Take(200).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    private static PieceJointeDto ToDto(PieceJointe p) =>
        new(p.Id, p.NumeroDocument, p.DateDocument, p.AnneeDocument, p.Emetteur,
            p.TypeDocument, p.Objet, p.NomFichier, p.HashFichier, p.DateAjout);
}
