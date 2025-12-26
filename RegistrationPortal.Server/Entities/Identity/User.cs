using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationPortal.Server.Entities.Identity;

[Table("USERS")]
public class User
{
    [Key]
    [Column("USER_ID")]
    public int Id { get; set; }

    [Required]
    [Column("USERNAME")]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Column("EMAIL")]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("PASSWORD_HASH")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("FIRST_NAME")]
    [StringLength(50)]
    public string? FirstName { get; set; }

    [Column("LAST_NAME")]
    [StringLength(50)]
    public string? LastName { get; set; }

    [Column("IS_ACTIVE")]
    public bool IsActive { get; set; } = true;

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    [Column("LAST_LOGIN_AT")]
    public DateTime? LastLoginAt { get; set; }

    [Column("BRANCH")]
    [StringLength(50)]
    public string? Branch { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
