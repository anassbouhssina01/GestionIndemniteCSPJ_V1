using GestionIndemnite.Domain.RHOrganisation.Exceptions;

namespace GestionIndemnite.Domain.RHOrganisation.Rules;

public static class RibRules
{
    public static void Validate(
        string codeBanque,
        string codeVille,
        string numeroCompte,
        string cle)
    {
        ValidateDigits(codeBanque, 3, "رمز البنك");
        ValidateDigits(codeVille, 3, "رمز المدينة");
        ValidateDigits(numeroCompte, 16, "رقم الحساب");
        ValidateDigits(cle, 2, "المفتاح");
    }

    private static void ValidateDigits(string value, int length, string fieldName)
    {
        if (string.IsNullOrEmpty(value) || value.Length != length)
            throw new RegleMetierException($"{fieldName} يجب أن يحتوي على {length} أرقام");

        foreach (var c in value)
        {
            if (c is < '0' or > '9')
                throw new RegleMetierException($"{fieldName} يجب أن يحتوي على أرقام فقط");
        }
    }
}
