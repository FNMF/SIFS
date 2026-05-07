using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIFS.Infrastructure.Persistence.Models;

[Table("model_health_checks")]
public partial class ModelHealthCheck
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("algo_model_id")]
    public int AlgoModelId { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("response_time_ms")]
    public int? ResponseTimeMs { get; set; }

    [Column("checked_at", TypeName = "datetime")]
    public DateTime CheckedAt { get; set; }

    [Column("failure_reason", TypeName = "text")]
    public string? FailureReason { get; set; }
}
