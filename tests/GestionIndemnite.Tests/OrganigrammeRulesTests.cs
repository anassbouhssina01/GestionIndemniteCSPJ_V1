using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Domain.RHOrganisation.Rules;

namespace GestionIndemnite.Tests;

public class OrganigrammeRulesTests
{
    [Theory]
    [InlineData(NiveauJuridiction.CJR, NiveauJuridiction.TPI)]
    [InlineData(NiveauJuridiction.TPI, NiveauJuridiction.CA)]
    [InlineData(NiveauJuridiction.CA, NiveauJuridiction.Cassation)]
    public void Niveau_valide_accepte(NiveauJuridiction child, NiveauJuridiction parent)
    {
        OrganigrammeRules.ValidateTribunalParent(child, parent);
    }

    [Fact]
    public void Cassation_sans_parent_acceptee()
    {
        OrganigrammeRules.ValidateTribunalParent(NiveauJuridiction.Cassation, null);
    }

    [Fact]
    public void Cassation_avec_parent_rejetee()
    {
        Assert.Throws<RegleMetierException>(
            () => OrganigrammeRules.ValidateTribunalParent(NiveauJuridiction.Cassation, NiveauJuridiction.CA));
    }

    [Fact]
    public void Non_cassation_sans_parent_rejetee()
    {
        Assert.Throws<RegleMetierException>(
            () => OrganigrammeRules.ValidateTribunalParent(NiveauJuridiction.TPI, null));
    }

    [Theory]
    [InlineData(NiveauJuridiction.CJR, NiveauJuridiction.CA)]
    [InlineData(NiveauJuridiction.CJR, NiveauJuridiction.Cassation)]
    [InlineData(NiveauJuridiction.TPI, NiveauJuridiction.CJR)]
    [InlineData(NiveauJuridiction.CA, NiveauJuridiction.TPI)]
    public void Niveau_invalide_rejete(NiveauJuridiction child, NiveauJuridiction parent)
    {
        Assert.Throws<RegleMetierException>(
            () => OrganigrammeRules.ValidateTribunalParent(child, parent));
    }

    [Fact]
    public void RequiredParentLevel_chain()
    {
        Assert.Equal(NiveauJuridiction.TPI, OrganigrammeRules.RequiredParentLevel(NiveauJuridiction.CJR));
        Assert.Equal(NiveauJuridiction.CA, OrganigrammeRules.RequiredParentLevel(NiveauJuridiction.TPI));
        Assert.Equal(NiveauJuridiction.Cassation, OrganigrammeRules.RequiredParentLevel(NiveauJuridiction.CA));
        Assert.Null(OrganigrammeRules.RequiredParentLevel(NiveauJuridiction.Cassation));
    }
}
