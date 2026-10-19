using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;

namespace GestionIndemnite.Domain.RHOrganisation.Rules;

public static class OrganigrammeRules
{
    public static NiveauJuridiction? RequiredParentLevel(NiveauJuridiction level) => level switch
    {
        NiveauJuridiction.CJR => NiveauJuridiction.TPI,
        NiveauJuridiction.TPI => NiveauJuridiction.CA,
        NiveauJuridiction.CA => NiveauJuridiction.Cassation,
        NiveauJuridiction.Cassation => null,
        _ => throw new RegleMetierException("مستوى الولاية القضائية غير معروف")
    };

    public static void ValidateTribunalParent(
        NiveauJuridiction childLevel,
        NiveauJuridiction? parentLevel)
    {
        if (childLevel == NiveauJuridiction.Cassation)
        {
            if (parentLevel is not null)
                throw new RegleMetierException("محكمة التمييز لا تخضع لأي هيئة عليا");
            return;
        }

        if (parentLevel is null)
            throw new RegleMetierException("المستوى الأعلى مطلوب لهذا النوع من المحاكم");

        var required = RequiredParentLevel(childLevel)!.Value;
        if (parentLevel.Value != required)
            throw new RegleMetierException("الاختصاص المباشر مطلوب: المستوى الأعلى يجب أن يكون المستوى الأعلى مباشرة");
    }

    public static bool IsCycle(Guid id, Guid? parentId, Func<Guid, Guid?> getParentAtDate, DateOnly date)
    {
        if (parentId is null)
            return false;

        if (parentId == id)
            return true;

        var current = parentId;
        var visited = new HashSet<Guid>();
        var guard = 0;

        while (current is not null && guard++ < 100)
        {
            if (!visited.Add(current.Value))
                return true;
            if (current == id)
                return true;
            current = getParentAtDate(current.Value);
        }

        return false;
    }
}
