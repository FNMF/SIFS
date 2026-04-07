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
    public Guid Id { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [Column("user_id")]
    [MaxLength(16)]
    public Guid UserId { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }
}
