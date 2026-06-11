using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Configuration;

namespace ContosoDashboard.Tests;

/// <summary>
/// Integration tests for document search and filter functionality (T030)
/// Tests document list retrieval, filtering by project/category, and search queries
/// </summary>
public class DocumentSearchTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;
    private DocumentService _documentService = null!;

    public async Task InitializeAsync()
    {
        // Create in-memory SQLite database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        // Create mocks for services
        var fileStorageMock = new Mock<IFileStorageService>();
        var notificationMock = new Mock<INotificationService>();
        var configMock = new Mock<IConfiguration>();

        _documentService = new DocumentService(_context, fileStorageMock.Object, notificationMock.Object, configMock.Object);

        // Seed test data
        await SeedTestData();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    private async Task SeedTestData()
    {
        var user1 = new User
        {
            UserId = 1,
            Email = "user1@contoso.com",
            DisplayName = "User One",
            Role = UserRole.Employee,
            CreatedDate = DateTime.UtcNow,
            EmailNotificationsEnabled = true,
            InAppNotificationsEnabled = true
        };

        var user2 = new User
        {
            UserId = 2,
            Email = "user2@contoso.com",
            DisplayName = "User Two",
            Role = UserRole.Employee,
            CreatedDate = DateTime.UtcNow,
            EmailNotificationsEnabled = true,
            InAppNotificationsEnabled = true
        };

        var project = new Project
        {
            ProjectId = 1,
            Name = "Test Project",
            ProjectManagerId = 1,
            StartDate = DateTime.UtcNow,
            Status = ProjectStatus.Active
        };

        var doc1 = new Document
        {
            DocumentId = 1,
            Title = "Q1 Report",
            Description = "Quarterly report for Q1",
            Category = "Report",
            Tags = "quarterly,financial,2024",
            ProjectId = 1,
            FileName = "q1_report.pdf",
            FilePath = "/uploads/1/1/doc1.pdf",
            FileType = "application/pdf",
            FileSize = 1024 * 500,
            UploadedAt = DateTime.UtcNow.AddDays(-5),
            UploadedBy = "user1@contoso.com",
            UploadedByUserId = 1
        };

        var doc2 = new Document
        {
            DocumentId = 2,
            Title = "Project Charter",
            Description = "Project charter and scope",
            Category = "Planning",
            Tags = "charter,scope,project",
            ProjectId = 1,
            FileName = "charter.docx",
            FilePath = "/uploads/1/1/doc2.docx",
            FileType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileSize = 1024 * 250,
            UploadedAt = DateTime.UtcNow.AddDays(-10),
            UploadedBy = "user1@contoso.com",
            UploadedByUserId = 1
        };

        var doc3 = new Document
        {
            DocumentId = 3,
            Title = "Team Agenda",
            Description = "Meeting agenda for team sync",
            Category = "Meeting",
            Tags = "meeting,agenda,team",
            ProjectId = null,
            FileName = "agenda.docx",
            FilePath = "/uploads/1/personal/doc3.docx",
            FileType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileSize = 1024 * 100,
            UploadedAt = DateTime.UtcNow.AddDays(-1),
            UploadedBy = "user1@contoso.com",
            UploadedByUserId = 1
        };

        var doc4 = new Document
        {
            DocumentId = 4,
            Title = "Shared Budget",
            Description = "Budget shared with team",
            Category = "Financial",
            Tags = "budget,financial,2024",
            ProjectId = 1,
            FileName = "budget.xlsx",
            FilePath = "/uploads/2/1/doc4.xlsx",
            FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileSize = 1024 * 300,
            UploadedAt = DateTime.UtcNow.AddDays(-3),
            UploadedBy = "user2@contoso.com",
            UploadedByUserId = 2
        };

        _context.Users.AddRange(user1, user2);
        _context.Projects.Add(project);
        _context.Documents.AddRange(doc1, doc2, doc3, doc4);

        // Add share for doc4 so user1 can access it
        var share = new DocumentShare
        {
            DocumentShareId = 1,
            DocumentId = 4,
            RecipientUserId = 1,
            SharedAt = DateTime.UtcNow.AddDays(-3),
            SharedBy = "user2@contoso.com",
            Permission = "View"
        };

        _context.DocumentShares.Add(share);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task SearchDocumentsAsync_ReturnsAccessibleDocuments_WhenNoFiltersProvided()
    {
        // Arrange
        var userId = 1;

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, null, null, null);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(4, results.Count); // User1 owns 3 docs + 1 shared = 4
    }

    [Fact]
    public async Task SearchDocumentsAsync_FiltersDocumentsByTitle_WhenQueryProvided()
    {
        // Arrange
        var userId = 1;
        var query = "Report";

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, query, null, null);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Contains("Report", r.Title));
    }

    [Fact]
    public async Task SearchDocumentsAsync_FiltersDocumentsByTags_WhenQueryProvided()
    {
        // Arrange
        var userId = 1;
        var query = "financial";

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, query, null, null);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count); // Q1 Report and Shared Budget
    }

    [Fact]
    public async Task SearchDocumentsAsync_FiltersDocumentsByProject_WhenProjectIdProvided()
    {
        // Arrange
        var userId = 1;
        var projectId = 1;

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, null, projectId, null);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count); // Q1 Report, Project Charter, Shared Budget
        Assert.All(results, r => Assert.Equal(1, r.ProjectId));
    }

    [Fact]
    public async Task SearchDocumentsAsync_FiltersDocumentsByCategory_WhenCategoryProvided()
    {
        // Arrange
        var userId = 1;
        var category = "Planning";

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, null, null, category);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("Project Charter", results.First().Title);
    }

    [Fact]
    public async Task SearchDocumentsAsync_ReturnsCombinedResults_WhenMultipleFiltersProvided()
    {
        // Arrange
        var userId = 1;
        var query = "project";
        var projectId = 1;
        var category = "Planning";

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, query, projectId, category);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("Project Charter", results.First().Title);
    }

    [Fact]
    public async Task SearchDocumentsAsync_IncludesSharedDocuments_WhenUserIsRecipient()
    {
        // Arrange
        var userId = 1;

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, null, null, null);

        // Assert
        Assert.NotNull(results);
        var sharedDoc = results.FirstOrDefault(d => d.DocumentId == 4);
        Assert.NotNull(sharedDoc);
        Assert.Equal("Shared Budget", sharedDoc.Title);
    }

    [Fact]
    public async Task SearchDocumentsAsync_ReturnsMostRecent_First()
    {
        // Arrange
        var userId = 1;

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, null, null, null);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        // Verify ordering: most recent first
        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.True(results[i].UploadedAt >= results[i + 1].UploadedAt);
        }
    }

    [Fact]
    public async Task SearchDocumentsAsync_ReturnsEmptyList_WhenNoDocumentsMatch()
    {
        // Arrange
        var userId = 1;
        var query = "NonexistentDocument";

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, query, null, null);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }
}

        var doc1 = new Document
        {
            DocumentId = 1,
            Title = "Q1 Report",
            Description = "Quarterly report for Q1",
            Category = "Report",
            Tags = "quarterly,financial,2024",
            ProjectId = 1,
            FileName = "q1_report.pdf",
            FilePath = "/uploads/1/1/doc1.pdf",
            FileType = "application/pdf",
            FileSize = 1024 * 500,
            UploadedAt = DateTime.UtcNow.AddDays(-5),
            UploadedBy = "user1@contoso.com",
            UploadedByUserId = 1
        };

        var doc2 = new Document
        {
            DocumentId = 2,
            Title = "Project Charter",
            Description = "Project charter and scope",
            Category = "Planning",
            Tags = "charter,scope,project",
            ProjectId = 1,
            FileName = "charter.docx",
            FilePath = "/uploads/1/1/doc2.docx",
            FileType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileSize = 1024 * 250,
            UploadedAt = DateTime.UtcNow.AddDays(-10),
            UploadedBy = "user1@contoso.com",
            UploadedByUserId = 1
        };

        var doc3 = new Document
        {
            DocumentId = 3,
            Title = "Team Agenda",
            Description = "Meeting agenda for team sync",
            Category = "Meeting",
            Tags = "meeting,agenda,team",
            ProjectId = null,
            FileName = "agenda.docx",
            FilePath = "/uploads/1/personal/doc3.docx",
            FileType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileSize = 1024 * 100,
            UploadedAt = DateTime.UtcNow.AddDays(-1),
            UploadedBy = "user1@contoso.com",
            UploadedByUserId = 1
        };

        var doc4 = new Document
        {
            DocumentId = 4,
            Title = "Shared Budget",
            Description = "Budget shared with team",
            Category = "Financial",
            Tags = "budget,financial,2024",
            ProjectId = 1,
            FileName = "budget.xlsx",
            FilePath = "/uploads/2/1/doc4.xlsx",
            FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileSize = 1024 * 300,
            UploadedAt = DateTime.UtcNow.AddDays(-3),
            UploadedBy = "user2@contoso.com",
            UploadedByUserId = 2
        };

        _context.Users.AddRange(user1, user2);
        _context.Projects.Add(project);
        _context.Documents.AddRange(doc1, doc2, doc3, doc4);

        // Add share for doc4 so user1 can access it
        var share = new DocumentShare
        {
            DocumentShareId = 1,
            DocumentId = 4,
            RecipientUserId = 1,
            SharedAt = DateTime.UtcNow.AddDays(-3),
            SharedBy = "user2@contoso.com",
            Permission = "View"
        };

        _context.DocumentShares.Add(share);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task SearchDocumentsAsync_ReturnsFiveHundredMostRecentDocuments_WhenAccessible()
    {
        // Arrange
        var userId = 1;

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, null, null, null);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count); // User1 owns 3 docs, can see 1 shared = 4, but top 500
    }

    [Fact]
    public async Task SearchDocumentsAsync_FiltersDocumentsByTitle_WhenQueryProvided()
    {
        // Arrange
        var userId = 1;
        var query = "Report";

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, query, null, null);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("Q1 Report", results.First().Title);
    }

    [Fact]
    public async Task SearchDocumentsAsync_FiltersDocumentsByTags_WhenQueryProvided()
    {
        // Arrange
        var userId = 1;
        var query = "financial";

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, query, null, null);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count); // Q1 Report and Shared Budget
    }

    [Fact]
    public async Task SearchDocumentsAsync_FiltersDocumentsByProject_WhenProjectIdProvided()
    {
        // Arrange
        var userId = 1;
        var projectId = 1;

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, null, projectId, null);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count); // Q1 Report, Project Charter (not Team Agenda or Shared Budget)
    }

    [Fact]
    public async Task SearchDocumentsAsync_FiltersDocumentsByCategory_WhenCategoryProvided()
    {
        // Arrange
        var userId = 1;
        var category = "Planning";

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, null, null, category);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("Project Charter", results.First().Title);
    }

    [Fact]
    public async Task SearchDocumentsAsync_ReturnsCombinedResults_WhenMultipleFiltersProvided()
    {
        // Arrange
        var userId = 1;
        var query = "project";
        var projectId = 1;
        var category = "Planning";

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, query, projectId, category);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("Project Charter", results.First().Title);
    }

    [Fact]
    public async Task SearchDocumentsAsync_IncludesSharedDocuments_WhenUserIsRecipient()
    {
        // Arrange
        var userId = 1;

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, null, null, null);

        // Assert
        Assert.NotNull(results);
        var sharedDoc = results.FirstOrDefault(d => d.DocumentId == 4);
        Assert.NotNull(sharedDoc);
        Assert.Equal("Shared Budget", sharedDoc.Title);
    }

    [Fact]
    public async Task SearchDocumentsAsync_ReturnsMostRecent_First()
    {
        // Arrange
        var userId = 1;

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, null, null, null);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        // Verify ordering: most recent first
        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.True(results[i].UploadedAt >= results[i + 1].UploadedAt);
        }
    }

    [Fact]
    public async Task SearchDocumentsAsync_ReturnsEmptyList_WhenNoDocumentsMatch()
    {
        // Arrange
        var userId = 1;
        var query = "NonexistentDocument";

        // Act
        var results = await _documentService.SearchDocumentsAsync(userId, query, null, null);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }
}
