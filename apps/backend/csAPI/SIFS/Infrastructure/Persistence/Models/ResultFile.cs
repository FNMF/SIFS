using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SIFS.Infrastructure.Persistence.Models;

[Table("result_file")]
public partial class ResultFile
{
    [Key]
    [Column("id")]
    [MaxLength(16)]
    public Guid Id { get; set; }

    [Column("algo_task_id")]
    [MaxLength(16)]
    public Guid AlgoTaskId { get; set; }

    [Column("algo_type")]
    public int AlgoType { get; set; }

    [Column("is_fake")]
    public bool? IsFake { get; set; }

    [Column("confidence")]
    public double? Confidence { get; set; }

    [Column("mask_local_url")]
    [StringLength(255)]
    public string? MaskLocalUrl { get; set; }

    [Column("mask_cloud_url")]
    [StringLength(255)]
    public string? MaskCloudUrl { get; set; }
}
