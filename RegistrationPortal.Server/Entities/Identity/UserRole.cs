using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationPortal.Server.Entities.Identity;

[Table("USER_ROLES")]
public class UserRole
{
    [Key]
    [Column("USER_ROLE_ID")]
    public int Id { get; set; }

    [Required]
    [Column("USER_ID")]
    public int UserId { get; set; }

    [Required]
    [Column("ROLE_ID")]
    public int RoleId { get; set; }

    [Column("ASSIGNED_AT")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [Column("ASSIGNED_BY")]
    public int? AssignedBy { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;
}
