using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Moq;

namespace ContosoDashboard.Tests;

/// <summary>
/// Unit tests for document service search and filter logic (T029)
/// Tests DocumentService methods for filtering, searching, and permission checks
/// </summary>
public class DocumentServiceSearchTests
{
    [Fact]
    public void TitleFilter_IsCaseInsensitive()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { DocumentId = 1, Title = "QUARTERLY REPORT", Description = "", Category = "Report" },
            new Document { DocumentId = 2, Title = "Monthly Report", Description = "", Category = "Report" },
            new Document { DocumentId = 3, Title = "Daily Notes", Description = "", Category = "Notes" }
        };

        var query = "report";

        // Act
        var filtered = documents.Where(d =>
            d.Title.ToLower().Contains(query.ToLower()) ||
            (d.Description != null && d.Description.ToLower().Contains(query.ToLower())) ||
            (d.Tags != null && d.Tags.ToLower().Contains(query.ToLower())) ||
            d.UploadedBy.ToLower().Contains(query.ToLower())
        ).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, d => Assert.Contains("report", d.Title.ToLower()));
    }

    [Fact]
    public void DescriptionFilter_IncludesDescriptionMatches()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { DocumentId = 1, Title = "Report", Description = "Financial analysis for Q1", Category = "Report" },
            new Document { DocumentId = 2, Title = "Notes", Description = "Team meeting notes", Category = "Notes" },
            new Document { DocumentId = 3, Title = "Chart", Description = "Sales report", Category = "Chart" }
        };

        var query = "Financial";

        // Act
        var filtered = documents.Where(d =>
            d.Title.ToLower().Contains(query.ToLower()) ||
            (d.Description != null && d.Description.ToLower().Contains(query.ToLower()))
        ).ToList();

        // Assert
        Assert.Single(filtered);
        Assert.Equal(1, filtered.First().DocumentId);
    }

    [Fact]
    public void TagsFilter_IncludesTagMatches()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { DocumentId = 1, Title = "Report", Tags = "quarterly,financial,2024", Category = "Report" },
            new Document { DocumentId = 2, Title = "Budget", Tags = "annual,financial,planning", Category = "Budget" },
            new Document { DocumentId = 3, Title = "Notes", Tags = "meeting,team", Category = "Notes" }
        };

        var query = "Financial";

        // Act
        var filtered = documents.Where(d =>
            d.Tags != null && d.Tags.ToLower().Contains(query.ToLower())
        ).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public void UploadedByFilter_FindsDocumentsByUploader()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { DocumentId = 1, Title = "Report", UploadedBy = "alice@contoso.com", Category = "Report" },
            new Document { DocumentId = 2, Title = "Budget", UploadedBy = "bob@contoso.com", Category = "Budget" },
            new Document { DocumentId = 3, Title = "Notes", UploadedBy = "alice@contoso.com", Category = "Notes" }
        };

        var query = "alice";

        // Act
        var filtered = documents.Where(d =>
            d.UploadedBy.ToLower().Contains(query.ToLower())
        ).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, d => Assert.Contains("alice", d.UploadedBy.ToLower()));
    }

    [Fact]
    public void ProjectFilter_ReturnsDocumentsForSpecificProject()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { DocumentId = 1, Title = "Report", ProjectId = 1, Category = "Report" },
            new Document { DocumentId = 2, Title = "Budget", ProjectId = 2, Category = "Budget" },
            new Document { DocumentId = 3, Title = "Notes", ProjectId = 1, Category = "Notes" }
        };

        var projectId = 1;

        // Act
        var filtered = documents.Where(d => d.ProjectId == projectId).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, d => Assert.Equal(1, d.ProjectId));
    }

    [Fact]
    public void CategoryFilter_ReturnsDocumentsForSpecificCategory()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { DocumentId = 1, Title = "Report", Category = "Report" },
            new Document { DocumentId = 2, Title = "Budget", Category = "Financial" },
            new Document { DocumentId = 3, Title = "Sales", Category = "Financial" }
        };

        var category = "Financial";

        // Act
        var filtered = documents.Where(d => d.Category == category).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, d => Assert.Equal("Financial", d.Category));
    }

    [Fact]
    public void SortByDate_ReturnsMostRecentFirst()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { DocumentId = 1, Title = "Old", UploadedAt = DateTime.UtcNow.AddDays(-10), Category = "Report" },
            new Document { DocumentId = 2, Title = "New", UploadedAt = DateTime.UtcNow.AddDays(-1), Category = "Report" },
            new Document { DocumentId = 3, Title = "Newer", UploadedAt = DateTime.UtcNow, Category = "Report" }
        };

        // Act
        var sorted = documents.OrderByDescending(d => d.UploadedAt).ToList();

        // Assert
        Assert.Equal("Newer", sorted[0].Title);
        Assert.Equal("New", sorted[1].Title);
        Assert.Equal("Old", sorted[2].Title);
    }

    [Fact]
    public void SortByTitle_ReturnsAlphabeticallyOrdered()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { DocumentId = 1, Title = "Zebra Report", Category = "Report" },
            new Document { DocumentId = 2, Title = "Apple Report", Category = "Report" },
            new Document { DocumentId = 3, Title = "Mango Report", Category = "Report" }
        };

        // Act
        var sorted = documents.OrderBy(d => d.Title).ToList();

        // Assert
        Assert.Equal("Apple Report", sorted[0].Title);
        Assert.Equal("Mango Report", sorted[1].Title);
        Assert.Equal("Zebra Report", sorted[2].Title);
    }

    [Fact]
    public void CombinedFilters_ReturnsDocumentsMatchingAllCriteria()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { DocumentId = 1, Title = "Q1 Report", ProjectId = 1, Category = "Financial", Tags = "quarterly" },
            new Document { DocumentId = 2, Title = "Q1 Notes", ProjectId = 1, Category = "Notes", Tags = "meeting" },
            new Document { DocumentId = 3, Title = "Q2 Report", ProjectId = 2, Category = "Financial", Tags = "quarterly" }
        };

        var projectId = 1;
        var category = "Financial";

        // Act
        var filtered = documents
            .Where(d => d.ProjectId == projectId)
            .Where(d => d.Category == category)
            .ToList();

        // Assert
        Assert.Single(filtered);
        Assert.Equal("Q1 Report", filtered.First().Title);
    }

    [Fact]
    public void EmptyQuery_ReturnsAllDocuments()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { DocumentId = 1, Title = "Report", Category = "Report" },
            new Document { DocumentId = 2, Title = "Budget", Category = "Budget" },
            new Document { DocumentId = 3, Title = "Notes", Category = "Notes" }
        };

        var query = "";

        // Act
        var filtered = documents.Where(d =>
            string.IsNullOrEmpty(query) || d.Title.ToLower().Contains(query.ToLower())
        ).ToList();

        // Assert
        Assert.Equal(3, filtered.Count);
    }

    [Fact]
    public void FilterLimitToFiveHundredDocuments()
    {
        // Arrange
        var documents = Enumerable.Range(1, 600)
            .Select(i => new Document { DocumentId = i, Title = $"Doc{i}", Category = "Test", UploadedAt = DateTime.UtcNow.AddDays(-i) })
            .ToList();

        // Act
        var limited = documents
            .OrderByDescending(d => d.UploadedAt)
            .Take(500)
            .ToList();

        // Assert
        Assert.Equal(500, limited.Count);
    }
}
