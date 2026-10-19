using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Enums;

namespace GestionIndemnite.Web.Models;

/// <summary>
/// Dossier complet d'un fonctionnaire : identité, RIB, situation actuelle
/// et tous ses historiques (grades, situations, affectations, responsabilités).
/// </summary>
public class DossierFonctionnaireViewModel
{
    public FonctionnaireDto Fonctionnaire { get; init; } = null!;

    public RibDto? RibActif { get; init; }
    public IReadOnlyList<RibDto> Ribs { get; init; } = Array.Empty<RibDto>();

    public IReadOnlyList<HistoriqueCadreGradeDto> GradePeriods { get; init; } = Array.Empty<HistoriqueCadreGradeDto>();
    public IReadOnlyList<SituationAdministrativeDto> SituationPeriods { get; init; } = Array.Empty<SituationAdministrativeDto>();
    public IReadOnlyList<HistoriqueAffectationDto> AffectationPeriods { get; init; } = Array.Empty<HistoriqueAffectationDto>();
    public IReadOnlyList<HistoriqueResponsabiliteDto> ResponsabilitePeriods { get; init; } = Array.Empty<HistoriqueResponsabiliteDto>();

    public IReadOnlyList<GradeDto> Grades { get; init; } = Array.Empty<GradeDto>();
    public IReadOnlyList<StatutAdministratifDto> Statuts { get; init; } = Array.Empty<StatutAdministratifDto>();
    public IReadOnlyList<TribunalDto> Tribunaux { get; init; } = Array.Empty<TribunalDto>();
    public IReadOnlyList<UniteOrganisationnelleDto> Unites { get; init; } = Array.Empty<UniteOrganisationnelleDto>();
    public IReadOnlyList<ResponsabiliteDto> Responsabilites { get; init; } = Array.Empty<ResponsabiliteDto>();
    public IReadOnlyList<PieceJointeDto> Documents { get; init; } = Array.Empty<PieceJointeDto>();
    public IReadOnlyList<CadreDto> Cadres { get; init; } = Array.Empty<CadreDto>();
    public IReadOnlyList<VilleDto> Villes { get; init; } = Array.Empty<VilleDto>();

    // ---- Situation actuelle (période ouverte, DateFin = null) ----
    public HistoriqueCadreGradeDto? GradeActuel =>
        GradePeriods.Where(p => p.DateFin is null)
            .OrderByDescending(p => p.DateDebut)
            .FirstOrDefault();

    public SituationAdministrativeDto? SituationActuelle =>
        SituationPeriods.Where(p => p.DateFin is null)
            .OrderByDescending(p => p.DateDebut)
            .FirstOrDefault();

    public HistoriqueAffectationDto? AffectationActuelle =>
        AffectationPeriods.Where(p => p.DateFin is null)
            .OrderByDescending(p => p.DateDebut)
            .FirstOrDefault();

    public IReadOnlyList<HistoriqueResponsabiliteDto> ResponsabilitesActuelles =>
        ResponsabilitePeriods.Where(p => p.DateFin is null)
            .OrderByDescending(p => p.DateDebut)
            .ToList();

    // ---- Résolution des libellés ----
    public GradeDto? GradeOf(Guid id) => Grades.FirstOrDefault(g => g.Id == id);

    public StatutAdministratifDto? StatutOf(Guid id) => Statuts.FirstOrDefault(s => s.Id == id);

    public TribunalDto? TribunalOf(Guid? id) =>
        id.HasValue ? Tribunaux.FirstOrDefault(t => t.Id == id.Value) : null;

    public UniteOrganisationnelleDto? UniteOf(Guid? id) =>
        id.HasValue ? Unites.FirstOrDefault(u => u.Id == id.Value) : null;

    public ResponsabiliteDto? ResponsabiliteOf(Guid id) =>
        Responsabilites.FirstOrDefault(r => r.Id == id);

    public CadreDto? CadreOf(Guid id) => Cadres.FirstOrDefault(c => c.Id == id);

    public VilleDto? VilleOf(Guid id) => Villes.FirstOrDefault(v => v.Id == id);

    /// <summary>Ville de la juridiction ou de l'unité d'affectation.</summary>
    public VilleDto? VilleOfAffectation(HistoriqueAffectationDto affectation)
    {
        if (affectation.IdTribunalAffectation.HasValue)
        {
            var tribunal = TribunalOf(affectation.IdTribunalAffectation);
            return tribunal is null ? null : VilleOf(tribunal.IdVille);
        }

        var unite = UniteOf(affectation.IdUniteAffectation);
        return unite is null ? null : VilleOf(unite.IdVille);
    }

    public PieceJointeDto? DocumentOf(Guid? id) =>
        id.HasValue ? Documents.FirstOrDefault(d => d.Id == id.Value) : null;

    public string AffectationLabel(HistoriqueAffectationDto affectation)
    {
        if (affectation.IdTribunalAffectation.HasValue)
            return TribunalOf(affectation.IdTribunalAffectation)?.NomTribunal ?? "—";

        return UniteOf(affectation.IdUniteAffectation)?.NomUnite ?? "—";
    }

    public string RattachementLabel(HistoriqueResponsabiliteDto responsabilite)
    {
        if (responsabilite.IdTribunal.HasValue)
            return TribunalOf(responsabilite.IdTribunal)?.NomTribunal ?? "—";

        return UniteOf(responsabilite.IdUniteOrganisationnelle)?.NomUnite ?? "—";
    }

    public static string SectionLabel(SectionTribunal? section) => section switch
    {
        SectionTribunal.Parquet => "Parquet",
        SectionTribunal.Presidence => "Présidence",
        _ => "—"
    };
}
