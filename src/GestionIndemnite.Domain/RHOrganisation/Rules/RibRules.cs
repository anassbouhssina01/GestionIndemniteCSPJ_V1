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
        ValidateDigits(codeBanque, 3, "Le code banque");
        ValidateDigits(codeVille, 3, "Le code ville");
        ValidateDigits(numeroCompte, 16, "Le numéro de compte");
        ValidateDigits(cle, 2, "La clé");
    }

    private static void ValidateDigits(string value, int length, string fieldName)
    {
        if (string.IsNullOrEmpty(value) || value.Length != length)
            throw new RegleMetierException($"{fieldName} doit contenir {length} chiffres.");

        foreach (var c in value)
        {
            if (c is < '0' or > '9')
                throw new RegleMetierException($"{fieldName} ne doit contenir que des chiffres.");
        }
    }
}
