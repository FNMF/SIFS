using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SIFS.Infrastructure.Persistence.Models;

[Table("localfile")]
public partial class Localfile
{
    [Key]
    [Column("id")]
    [MaxLength(16)]
    public byte[] Id { get; set; } = null!;

    [Column("url_local")]
    [StringLength(255)]
    public string UrlLocal { get; set; } = null!;

    [Column("url_cloud")]
    [StringLength(255)]
    public string? UrlCloud { get; set; }

    [Column("algo_task_id")]
    [StringLength(255)]
    public string AlgoTaskId { get; set; } = null!;
}
