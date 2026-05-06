using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIFS.Infrastructure.Persistence.Models;

[Table("task_audits")]
public partial class TaskAudit
{
    [Key]
    [Column("id")]
    [MaxLength(16)]
    public Guid Id { get; set; }

    [Column("task_id")]
    [MaxLength(16)]
    public Guid TaskId { get; set; }

    [Column("from_status")]
    [StringLength(50)]
    public string? FromStatus { get; set; }

    [Column("to_status")]
    [StringLength(50)]
    public string ToStatus { get; set; } = null!;

    [Column("reason", TypeName = "text")]
    public string? Reason { get; set; }

    [Column("operator_id")]
    [MaxLength(16)]
    public Guid? OperatorId { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column("extra_json", TypeName = "text")]
    public string? ExtraJson { get; set; }
}
