namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, int userId, int? projectId);
    Task<Stream?> OpenReadAsync(string storagePath);
    Task<bool> DeleteFileAsync(string storagePath);
    Task<bool> FileExistsAsync(string storagePath);
}
