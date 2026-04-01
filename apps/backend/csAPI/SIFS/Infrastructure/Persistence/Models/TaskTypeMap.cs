using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SIFS.Infrastructure.Persistence.Models;

[Table("task_type_map")]
public partial class TaskTypeMap
{
    [Key]
    [Column("id")]
    [MaxLength(16)]
    public byte[] Id { get; set; } = null!;

    [Column("task_id")]
    [MaxLength(16)]
    public byte[] TaskId { get; set; } = null!;

    [Column("type_id")]
    public int TypeId { get; set; }
}
