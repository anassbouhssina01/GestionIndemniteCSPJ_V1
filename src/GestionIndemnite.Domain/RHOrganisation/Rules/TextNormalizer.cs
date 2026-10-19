using System.Text;

namespace GestionIndemnite.Domain.RHOrganisation.Rules;

public static class TextNormalizer
{
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length);
        var previousWasSpace = false;

        foreach (var ch in input.Trim())
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!previousWasSpace)
                {
                    sb.Append(' ');
                    previousWasSpace = true;
                }
                continue;
            }

            previousWasSpace = false;

            var c = ch switch
            {
                'أ' or 'إ' or 'آ' => 'ا',
                'ى' => 'ي',
                'ة' => 'ه',
                _ => ch
            };

            if (c is '\u0640' || (c >= '\u064B' && c <= '\u065F') || c == '\u0670')
                continue;

            sb.Append(char.ToUpperInvariant(c));
        }

        return sb.ToString();
    }
}
