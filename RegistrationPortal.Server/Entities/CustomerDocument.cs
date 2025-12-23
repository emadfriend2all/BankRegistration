using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationPortal.Server.Entities
{
    [Table("customer_documents", Schema = "SSDBONLINE")]
    public class CustomerDocument
    {
        [Key]
        [Column("id", TypeName = "VARCHAR2(50 BYTE)")]
        [Required]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;

        [Column("customer_id", TypeName = "VARCHAR2(50 BYTE)")]
        [Required]
        [StringLength(50)]
        public string CustomerId { get; set; } = string.Empty;

        [Column("document_type", TypeName = "VARCHAR2(50 BYTE)")]
        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        [Column("file_url", TypeName = "VARCHAR2(500 BYTE)")]
        [Required]
        [StringLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [Column("original_file_name", TypeName = "VARCHAR2(255 BYTE)")]
        [StringLength(255)]
        public string? OriginalFileName { get; set; }

        [Column("file_size", TypeName = "NUMBER")]
        public long FileSize { get; set; }

        [Column("mime_type", TypeName = "VARCHAR2(100 BYTE)")]
        [StringLength(100)]
        public string? MimeType { get; set; }

        [Column("created_at", TypeName = "TIMESTAMP(6)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at", TypeName = "TIMESTAMP(6)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("CustomerId")]
        public virtual CustMast Customer { get; set; } = null!;
    }

    public static class DocumentTypes
    {
        public const string Identification = "Identification";
        public const string NationalId = "NationalId";
        public const string PersonalImage = "PersonalImage";
        public const string ImageFortheRequesterHoldingTheID = "ImageFortheRequesterHoldingTheID";
        public const string SignitureTemplate = "SignitureTemplate";
        public const string HandwrittenRequest = "HandwrittenRequest";
        public const string EmploymentCertificate = "EmploymentCertificate";
    }
}
