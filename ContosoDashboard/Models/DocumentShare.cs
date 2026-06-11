using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentShare
{
    [Key]
    public int DocumentShareId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    public int? RecipientUserId { get; set; }
    public int? RecipientTeamId { get; set; }

    [Required]
    public DateTimeOffset SharedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [MaxLength(255)]
    public string SharedBy { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Permission { get; set; } = "read";

    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;

    [ForeignKey(nameof(RecipientUserId))]
    public virtual User? RecipientUser { get; set; }
}
