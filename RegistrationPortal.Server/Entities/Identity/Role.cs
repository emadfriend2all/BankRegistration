using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationPortal.Server.Entities.Identity;

[Table("ROLES")]
public class Role
{
    [Key]
    [Column("ROLE_ID")]
    public int Id { get; set; }

    [Required]
    [Column("NAME")]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("DESCRIPTION")]
    [StringLength(200)]
    public string? Description { get; set; }

    [Column("IS_ACTIVE")]
    public bool IsActive { get; set; } = true;

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
