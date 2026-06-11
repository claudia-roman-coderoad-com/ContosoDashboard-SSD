using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Azure.Storage.Queues;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<Document> UploadDocumentAsync(Stream fileStream, string fileName, string contentType, long fileSize, string title, string category, string? description, string? tags, int? projectId, int uploadedByUserId, string uploadedBy);
    Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId);
    Task<List<Document>> SearchDocumentsAsync(int userId, string? query, int? projectId, string? category);
    Task<bool> UpdateDocumentMetadataAsync(int documentId, int requestingUserId, string title, string category, string? description, string? tags);
    Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId);
}

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _configuration;

    private static readonly string[] AllowedExtensions = new[]
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".jpeg", ".jpg", ".png"
    };

    public DocumentService(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        INotificationService notificationService,
        IConfiguration configuration)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
        _configuration = configuration;
    }

    public async Task<Document> UploadDocumentAsync(Stream fileStream, string fileName, string contentType, long fileSize, string title, string category, string? description, string? tags, int? projectId, int uploadedByUserId, string uploadedBy)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }

        if (fileSize > 25 * 1024 * 1024)
        {
            throw new InvalidOperationException("File size exceeds the maximum allowed size of 25 MB.");
        }

        var storagePath = await _fileStorageService.SaveFileAsync(fileStream, fileName, contentType, uploadedByUserId, projectId);

        var document = new Document
        {
            Title = title,
            Description = description,
            Category = category,
            Tags = tags,
            ProjectId = projectId,
            UploadedByUserId = uploadedByUserId,
            UploadedBy = uploadedBy,
            UploadedAt = DateTimeOffset.UtcNow,
            FileName = fileName,
            FilePath = storagePath,
            FileType = contentType,
            FileSize = fileSize,
            ScanStatus = DocumentScanStatus.Pending
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        // Enqueue virus scan if configured
        await EnqueueVirusScanAsync(document.DocumentId, storagePath);

        return document;
    }

    public async Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Shares)
            .Include(d => d.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null) return null;

        if (!await UserHasAccessAsync(document, requestingUserId))
        {
            return null;
        }

        return document;
    }

    public async Task<List<Document>> SearchDocumentsAsync(int userId, string? query, int? projectId, string? category)
    {
        var accessibleProjectIds = await _context.Projects
            .Where(p => p.ProjectManagerId == userId)
            .Select(p => p.ProjectId)
            .Union(_context.ProjectMembers
                .Where(pm => pm.UserId == userId)
                .Select(pm => pm.ProjectId))
            .Distinct()
            .ToListAsync();

        var documents = await _context.Documents
            .Include(d => d.Project)
            .Include(d => d.Shares)
            .ToListAsync();

        if (!string.IsNullOrEmpty(query))
        {
            var normalized = query.Trim().ToLowerInvariant();
            documents = documents.Where(d =>
                d.Title.ToLower().Contains(normalized) ||
                (d.Description != null && d.Description.ToLower().Contains(normalized)) ||
                (d.Tags != null && d.Tags.ToLower().Contains(normalized)) ||
                d.UploadedBy.ToLower().Contains(normalized))
                .ToList();
        }

        documents = documents.Where(d =>
            d.UploadedByUserId == userId
            || (d.ProjectId.HasValue && accessibleProjectIds.Contains(d.ProjectId.Value))
            || d.Shares.Any(share => share.RecipientUserId == userId || (share.RecipientTeamId.HasValue && accessibleProjectIds.Contains(share.RecipientTeamId.Value)))
        ).ToList();

        if (projectId.HasValue)
        {
            documents = documents.Where(d => d.ProjectId == projectId.Value).ToList();
        }

        if (!string.IsNullOrEmpty(category))
        {
            documents = documents.Where(d => d.Category == category).ToList();
        }

        return documents
            .OrderByDescending(d => d.UploadedAt)
            .Take(500)
            .ToList();
    }

    public async Task<bool> UpdateDocumentMetadataAsync(int documentId, int requestingUserId, string title, string category, string? description, string? tags)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null) return false;

        if (!await UserHasAccessAsync(document, requestingUserId))
        {
            return false;
        }

        document.Title = title;
        document.Category = category;
        document.Description = description;
        document.Tags = tags;

        _context.Documents.Update(document);
        await _context.SaveChangesAsync();

        await CreateAuditAsync(documentId, requestingUserId, "MetadataUpdated", "Document metadata updated.");
        return true;
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null) return false;

        if (!await UserHasAccessAsync(document, requestingUserId))
        {
            return false;
        }

        await _fileStorageService.DeleteFileAsync(document.FilePath);

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();

        await CreateAuditAsync(documentId, requestingUserId, "Deleted", "Document deleted.");
        return true;
    }

    private async Task<bool> UserHasAccessAsync(Document document, int userId)
    {
        if (document.UploadedByUserId == userId)
        {
            return true;
        }

        if (document.Project != null)
        {
            if (document.Project.ProjectManagerId == userId) return true;
            if (document.Project.ProjectMembers.Any(pm => pm.UserId == userId)) return true;
        }

        if (document.Shares.Any(share =>
            share.RecipientUserId == userId ||
            (share.RecipientTeamId.HasValue && UserIsTeamMember(userId, share.RecipientTeamId.Value))))
        {
            return true;
        }

        return false;
    }

    private bool UserIsTeamMember(int userId, int teamId)
    {
        // This system currently uses project membership as a team proxy.
        return _context.ProjectMembers.Any(pm => pm.UserId == userId && pm.ProjectId == teamId);
    }

    private async Task CreateAuditAsync(int documentId, int userId, string action, string details)
    {
        var user = await _context.Users.FindAsync(userId);
        var audit = new DocumentAudit
        {
            DocumentId = documentId,
            Action = action,
            PerformedBy = user?.DisplayName ?? "Unknown",
            PerformedAt = DateTimeOffset.UtcNow,
            Details = details
        };

        _context.DocumentAudits.Add(audit);
        await _context.SaveChangesAsync();
    }

    private async Task EnqueueVirusScanAsync(int documentId, string storagePath)
    {
        var queueConnection = _configuration["AzureQueue:ConnectionString"];
        var queueName = _configuration["AzureQueue:ScanQueueName"] ?? "document-scan-requests";

        if (string.IsNullOrEmpty(queueConnection))
        {
            return; // No queue configured; skip async scan in local training mode.
        }

        var message = new
        {
            DocumentId = documentId,
            StoragePath = storagePath,
            RequestedAt = DateTimeOffset.UtcNow
        };

        var queueClient = new Azure.Storage.Queues.QueueClient(queueConnection, queueName);
        await queueClient.CreateIfNotExistsAsync();
        await queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message))));
    }
}
