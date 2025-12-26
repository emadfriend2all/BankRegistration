using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationPortal.Server.Entities.Identity;

[Table("PERMISSIONS")]
public class Permission
{
    [Key]
    [Column("PERMISSION_ID")]
    public int Id { get; set; }

    [Required]
    [Column("NAME")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("DESCRIPTION")]
    [StringLength(200)]
    public string? Description { get; set; }

    [Column("MODULE")]
    [StringLength(50)]
    public string? Module { get; set; }

    [Column("ACTION")]
    [StringLength(50)]
    public string? Action { get; set; }

    [Column("IS_ACTIVE")]
    public bool IsActive { get; set; } = true;

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
