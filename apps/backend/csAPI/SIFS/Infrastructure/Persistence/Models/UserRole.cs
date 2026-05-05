using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIFS.Infrastructure.Persistence.Models;

[Table("user_roles")]
public partial class UserRole
{
    [Key]
    [Column("id")]
    [MaxLength(16)]
    public Guid Id { get; set; }

    [Column("user_id")]
    [MaxLength(16)]
    public Guid UserId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }
}
