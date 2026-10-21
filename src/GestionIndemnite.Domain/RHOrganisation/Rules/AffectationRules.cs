using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;

namespace GestionIndemnite.Domain.RHOrganisation.Rules;

public static class AffectationRules
{
    public static void Validate(
        Guid? idTribunalAffectation,
        Guid? idUniteAffectation,
        Guid? idUniteTravail,
        SectionTribunal? sectionTribunal)
    {
        var tribunalSet = idTribunalAffectation.HasValue;
        var uniteSet = idUniteAffectation.HasValue;

        if (tribunalSet == uniteSet)
            throw new RegleMetierException("Vous devez sélectionner exactement un tribunal ou une unité organisationnelle.");

        if (sectionTribunal.HasValue && !tribunalSet)
            throw new RegleMetierException("La section n'est autorisée que pour une affectation à un tribunal.");

        if (idUniteTravail.HasValue && !tribunalSet)
            throw new RegleMetierException("L'unité de travail n'est autorisée que pour une affectation à un tribunal.");
    }
}
