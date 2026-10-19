using GestionIndemnite.Domain.RHOrganisation.Common;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class Rib : AuditableEntity
{
    public Guid IdFonctionnaire { get; set; }
    public string CodeBanque { get; set; } = string.Empty;
    public string CodeVille { get; set; } = string.Empty;
    public string NumeroCompte { get; set; } = string.Empty;
    public string Cle { get; set; } = string.Empty;
    public bool EstActif { get; set; }
}
