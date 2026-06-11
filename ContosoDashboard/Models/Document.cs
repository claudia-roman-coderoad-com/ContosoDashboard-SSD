using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public enum DocumentScanStatus
{
    Pending,
    Passed,
    Failed,
    Unknown
}

public class Document
{
    [Key]
    public int DocumentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Tags { get; set; }

    public int? ProjectId { get; set; }

    [Required]
    public int UploadedByUserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string UploadedBy { get; set; } = string.Empty;

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1024)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DocumentScanStatus ScanStatus { get; set; } = DocumentScanStatus.Pending;

    [MaxLength(1024)]
    public string? ScanMessage { get; set; }

    public bool IsQuarantined { get; set; } = false;

    [ForeignKey(nameof(UploadedByUserId))]
    public virtual User UploadedByUser { get; set; } = null!;

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    public virtual ICollection<DocumentShare> Shares { get; set; } = new List<DocumentShare>();
    public virtual ICollection<DocumentAudit> Audits { get; set; } = new List<DocumentAudit>();
}
