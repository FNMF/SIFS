using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SIFS.Infrastructure.Persistence.Models;

[Table("algo_task")]
public partial class AlgoTask
{
    [Key]
    [Column("id")]
    [MaxLength(16)]
    public Guid Id { get; set; }

    [Column("task_id")]
    [MaxLength(16)]
    public Guid TaskId { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [Column("started_at", TypeName = "datetime")]
    public DateTime? StartedAt { get; set; }

    [Column("finished_at", TypeName = "datetime")]
    public DateTime? FinishedAt { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [Column("algo_model_id")]
    public int? AlgoModelId { get; set; }

    [Column("algo_name")]
    [StringLength(100)]
    public string? AlgoName { get; set; }

    [Column("algo_api_url")]
    [StringLength(512)]
    public string? AlgoApiUrl { get; set; }

    [Column("failure_reason", TypeName = "text")]
    public string? FailureReason { get; set; }

    [Column("deleted_at", TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }
}
