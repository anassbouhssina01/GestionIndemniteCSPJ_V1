using GestionIndemnite.Application.RHOrganisation.Common;
using Microsoft.Extensions.Logging;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Files;

public class FileStorageService : IFileStorage
{
    private readonly string _rootPath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(string rootPath, ILogger<FileStorageService> logger)
    {
        _rootPath = rootPath;
        _logger = logger;
        Directory.CreateDirectory(rootPath);
    }

    public Task<string> SaveAsync(Stream content, string originalFileName, CancellationToken ct = default)
    {
        var storedName = $"{Guid.NewGuid()}.pdf";
        var fullPath = Path.Combine(_rootPath, storedName);

        using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        content.CopyTo(fs);

        _logger.LogInformation("Fichier stocké: {StoredName}", storedName);
        return Task.FromResult(storedName);
    }

    public Task<Stream> OpenReadAsync(string storedFileName, CancellationToken ct = default)
    {
        var safeName = Path.GetFileName(storedFileName);
        var fullPath = Path.Combine(_rootPath, safeName);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("الملف غير موجود", safeName);

        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public void Delete(string storedFileName)
    {
        var safeName = Path.GetFileName(storedFileName);
        var fullPath = Path.Combine(_rootPath, safeName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Fichier supprimé: {StoredName}", safeName);
        }
    }
}
