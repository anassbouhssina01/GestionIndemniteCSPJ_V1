using GestionIndemnite.Domain.RHOrganisation.Enums;

namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record ImportOptions(bool ModeValidationSeule, string NomFichier);

public record ImportResultatDto(
    Guid Id,
    string NomFichier,
    bool ModeValidationSeule,
    int NombreLignes,
    int NombreErreurs,
    string ContenuRapport);

public record ErreurLigne(int NumeroLigne, string Feuille, string Colonne, string Motif);

public record LigneFonctionnaire(
    int NumeroLigne,
    string Matricule,
    string Cin,
    string NomAr,
    string PrenomAr,
    string NomFr,
    string PrenomFr,
    DateOnly DateNaissance,
    DateOnly DateRecrutement,
    Genre Genre);

public record LigneRib(
    int NumeroLigne,
    string Matricule,
    string CodeBanque,
    string CodeVille,
    string NumeroCompte,
    string Cle);

public record LigneGrade(
    int NumeroLigne,
    string Matricule,
    string CodeCadre,
    int NumeroReferencie,
    DateOnly DateDebut,
    DateOnly? DateFin);

public record LigneSituation(
    int NumeroLigne,
    string Matricule,
    string CodeStatut,
    DateOnly DateDebut,
    DateOnly? DateFin);

public record LigneAffectation(
    int NumeroLigne,
    string Matricule,
    string NomTribunal,
    string NomStructure,
    string NomStructureTravail,
    SectionTribunal? Section,
    DateOnly DateDebut,
    DateOnly? DateFin);

public record LigneResponsabilite(
    int NumeroLigne,
    string Matricule,
    string CodeResponsabilite,
    string NomTribunal,
    string NomStructure,
    DateOnly DateDebut,
    DateOnly? DateFin);

public record ImportWorkbook(
    int NombreLignes,
    IReadOnlyList<LigneFonctionnaire> Fonctionnaires,
    IReadOnlyList<LigneRib> Ribs,
    IReadOnlyList<LigneGrade> Grades,
    IReadOnlyList<LigneSituation> Situations,
    IReadOnlyList<LigneAffectation> Affectations,
    IReadOnlyList<LigneResponsabilite> Responsabilites,
    IReadOnlyList<ErreurLigne> ErreursExtraction);
