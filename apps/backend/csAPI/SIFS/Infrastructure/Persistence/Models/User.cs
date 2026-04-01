using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SIFS.Infrastructure.Persistence.Models;

[Table("user")]
public partial class User
{
    [Key]
    [Column("id")]
    [MaxLength(16)]
    public Guid Id { get; set; }

    [Column("password_hashed")]
    [StringLength(255)]
    public string PasswordHashed { get; set; } = null!;

    [Column("salt")]
    [StringLength(255)]
    public string Salt { get; set; } = null!;

    [Column("account")]
    [StringLength(255)]
    public string Account { get; set; } = null!;
}
