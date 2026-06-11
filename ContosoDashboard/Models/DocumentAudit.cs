using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentAudit
{
    [Key]
    public int DocumentAuditId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PerformedBy { get; set; } = string.Empty;

    public DateTimeOffset PerformedAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(2000)]
    public string? Details { get; set; }

    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;
}
