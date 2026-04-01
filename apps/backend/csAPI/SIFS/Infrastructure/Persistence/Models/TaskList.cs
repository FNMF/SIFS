using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SIFS.Infrastructure.Persistence.Models;

[Table("task_list")]
public partial class TaskList
{
    [Key]
    [Column("id")]
    [MaxLength(16)]
    public byte[] Id { get; set; } = null!;

    [Column("status")]
    public int Status { get; set; }
}
