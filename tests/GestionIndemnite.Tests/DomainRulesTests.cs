using GestionIndemnite.Domain.RHOrganisation.Rules;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;

namespace GestionIndemnite.Tests;

public class TextNormalizerTests
{
    [Theory]
    [InlineData("أحمد", "احمد")]
    [InlineData("إبراهيم", "ابراهيم")]
    [InlineData("آمنة", "امنه")]
    [InlineData("مصطفى", "مصطفي")]
    [InlineData("آية", "ايه")]
    public void Normalisation_arabe(string input, string expected)
    {
        Assert.Equal(expected, TextNormalizer.Normalize(input));
    }

    [Fact]
    public void Normalisation_espaces_multiples()
    {
        Assert.Equal("TEST ONE", TextNormalizer.Normalize("  test    one  "));
    }

    [Fact]
    public void Normalisation_diacritiques_supprimees()
    {
        Assert.Equal("السلام عليكم", TextNormalizer.Normalize("السَّلَامُ عَلَيْكُمْ"));
    }

    [Fact]
    public void Normalisation_casse_uniforme()
    {
        Assert.Equal("HELLO WORLD", TextNormalizer.Normalize("hello world"));
    }

    [Fact]
    public void Normalisation_tatwil_supprime()
    {
        Assert.Equal("كتاب", TextNormalizer.Normalize("كـتـاب"));
    }

    [Fact]
    public void Normalisation_vide()
    {
        Assert.Equal(string.Empty, TextNormalizer.Normalize("   "));
    }
}

public class RibRulesTests
{
    [Fact]
    public void Rib_valide_accepte()
    {
        RibRules.Validate("011", "123", "0123456789012345", "12");
    }

    [Theory]
    [InlineData("01", "123", "0123456789012345", "12")]
    [InlineData("0111", "123", "0123456789012345", "12")]
    [InlineData("01A", "123", "0123456789012345", "12")]
    [InlineData("011", "12", "0123456789012345", "12")]
    [InlineData("011", "123", "012345678901234", "12")]
    [InlineData("011", "123", "0123456789012345", "1")]
    [InlineData("011", "123", "01234567890123AB", "12")]
    [InlineData("", "123", "0123456789012345", "12")]
    public void Rib_invalide_rejete(string cb, string cv, string nc, string cle)
    {
        Assert.Throws<RegleMetierException>(() => RibRules.Validate(cb, cv, nc, cle));
    }

    [Fact]
    public void Rib_zeros_en_tete_conserves()
    {
        RibRules.Validate("001", "002", "0000000000000099", "00");
    }
}
