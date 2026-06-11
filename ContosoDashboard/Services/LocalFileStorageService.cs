using System.Security.Cryptography;
using ContosoDashboard.Services;

namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public LocalFileStorageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    private string GetBaseUploadsFolder()
    {
        var configured = _configuration["FileStorage:UploadPath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(configured);
        }

        return Path.Combine(_environment.ContentRootPath, "AppData", "uploads");
    }

    private static string GetSafeFileName(string fileName)
    {
        return string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, int userId, int? projectId)
    {
        var baseFolder = GetBaseUploadsFolder();
        var projectFolder = projectId.HasValue ? projectId.Value.ToString() : "personal";
        var userFolder = userId.ToString();
        var extension = Path.GetExtension(fileName);
        var guidName = Guid.NewGuid().ToString("N") + extension;
        var storageRelativePath = Path.Combine(userFolder, projectFolder, guidName);
        var fullPath = Path.Combine(baseFolder, storageRelativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (directory == null)
        {
            throw new InvalidOperationException("Unable to determine directory path for uploads.");
        }

        Directory.CreateDirectory(directory);

        await using var fileDestination = File.Create(fullPath);
        await fileStream.CopyToAsync(fileDestination);
        await fileDestination.FlushAsync();

        return storageRelativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

    public Task<Stream?> OpenReadAsync(string storagePath)
    {
        var fullPath = Path.Combine(GetBaseUploadsFolder(), storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath)) return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }

    public Task<bool> DeleteFileAsync(string storagePath)
    {
        var fullPath = Path.Combine(GetBaseUploadsFolder(), storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath)) return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public Task<bool> FileExistsAsync(string storagePath)
    {
        var fullPath = Path.Combine(GetBaseUploadsFolder(), storagePath.Replace('/', Path.DirectorySeparatorChar));
        return Task.FromResult(File.Exists(fullPath));
    }
}
