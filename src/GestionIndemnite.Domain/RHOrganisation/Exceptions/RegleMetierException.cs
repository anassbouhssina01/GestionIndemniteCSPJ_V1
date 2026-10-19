namespace GestionIndemnite.Domain.RHOrganisation.Exceptions;

public class RegleMetierException : Exception
{
    public RegleMetierException(string message) : base(message)
    {
    }

    public RegleMetierException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
