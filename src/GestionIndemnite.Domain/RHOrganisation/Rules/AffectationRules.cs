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
            throw new RegleMetierException("يجب تحديد المحكمة أو الهيكل بدقة واحدة");

        if (sectionTribunal.HasValue && !tribunalSet)
            throw new RegleMetierException("القسم مسموح به فقط في حالة التعيين لمحكمة");

        if (idUniteTravail.HasValue && !tribunalSet)
            throw new RegleMetierException("هيكل العمل مسموح به فقط في حالة التعيين لمحكمة");
    }
}
