using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationPortal.Server.Entities.Identity;

[Table("ROLE_PERMISSIONS")]
public class RolePermission
{
    [Key]
    [Column("ROLE_PERMISSION_ID")]
    public int Id { get; set; }

    [Required]
    [Column("ROLE_ID")]
    public int RoleId { get; set; }

    [Required]
    [Column("PERMISSION_ID")]
    public int PermissionId { get; set; }

    [Column("GRANTED_AT")]
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    [Column("GRANTED_BY")]
    public int? GrantedBy { get; set; }

    // Navigation properties
    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;

    [ForeignKey("PermissionId")]
    public virtual Permission Permission { get; set; } = null!;
}
