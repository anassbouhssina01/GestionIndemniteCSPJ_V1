namespace GestionIndemnite.Application.RHOrganisation.Common;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string originalFileName, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string storedFileName, CancellationToken ct = default);
    void Delete(string storedFileName);
}
