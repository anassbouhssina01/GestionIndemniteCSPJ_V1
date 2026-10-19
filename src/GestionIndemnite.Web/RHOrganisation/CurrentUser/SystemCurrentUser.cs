using GestionIndemnite.Application.RHOrganisation.Common;

namespace GestionIndemnite.Web.RHOrganisation.CurrentUser;

public class SystemCurrentUser : ICurrentUser
{
    public string UserName => "system";
}
