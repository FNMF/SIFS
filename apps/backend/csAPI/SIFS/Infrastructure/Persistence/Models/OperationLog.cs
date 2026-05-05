using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIFS.Infrastructure.Persistence.Models;

[Table("operation_logs")]
public partial class OperationLog
{
    [Key]
    [Column("id")]
    [MaxLength(16)]
    public Guid Id { get; set; }

    [Column("actor_id")]
    [MaxLength(16)]
    public Guid? ActorId { get; set; }

    [Column("actor_username")]
    [StringLength(255)]
    public string? ActorUsername { get; set; }

    [Column("operation_type")]
    [StringLength(100)]
    public string OperationType { get; set; } = null!;

    [Column("target_type")]
    [StringLength(100)]
    public string? TargetType { get; set; }

    [Column("target_id")]
    [StringLength(100)]
    public string? TargetId { get; set; }

    [Column("request_ip")]
    [StringLength(64)]
    public string? RequestIp { get; set; }

    [Column("request_method")]
    [StringLength(16)]
    public string? RequestMethod { get; set; }

    [Column("request_path")]
    [StringLength(512)]
    public string? RequestPath { get; set; }

    [Column("request_summary", TypeName = "text")]
    public string? RequestSummary { get; set; }

    [Column("success")]
    public bool Success { get; set; } = true;

    [Column("failure_reason", TypeName = "text")]
    public string? FailureReason { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
