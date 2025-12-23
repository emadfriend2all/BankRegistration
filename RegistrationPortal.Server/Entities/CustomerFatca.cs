using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationPortal.Server.Entities
{
    [Table("customer_fatca", Schema = "SSDBONLINE")]
    public class CustomerFatca
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

        // SECTION A: US Person Status
        [Column("is_us_person", TypeName = "CHAR(1 BYTE)")]
        [StringLength(1)]
        public string? IsUsPerson { get; set; } // 'Y' or 'N'

        [Column("citizenship_countries", TypeName = "VARCHAR2(500 BYTE)")]
        [StringLength(500)]
        public string? CitizenshipCountries { get; set; } // Comma-separated list of countries

        [Column("has_immigrant_visa", TypeName = "CHAR(1 BYTE)")]
        [StringLength(1)]
        public string? HasImmigrantVisa { get; set; } // 'Y' or 'N'

        [Column("permanent_residency_states", TypeName = "VARCHAR2(500 BYTE)")]
        [StringLength(500)]
        public string? PermanentResidencyStates { get; set; } // Comma-separated list of states

        [Column("has_other_countries_residency", TypeName = "CHAR(1 BYTE)")]
        [StringLength(1)]
        public string? HasOtherCountriesResidency { get; set; } // 'Y' or 'N'

        [Column("sole_sudan_residency_confirmed", TypeName = "CHAR(10 BYTE)")]
        [StringLength(10)]
        public string? SoleSudanResidencyConfirmed { get; set; } // 'Y' or 'N'

        // SECTION B: US Tax Identification Numbers
        [Column("ssn", TypeName = "VARCHAR2(20 BYTE)")]
        [StringLength(20)]
        public string? SSN { get; set; }

        [Column("itin", TypeName = "VARCHAR2(20 BYTE)")]
        [StringLength(20)]
        public string? ITIN { get; set; }

        [Column("atin", TypeName = "VARCHAR2(20 BYTE)")]
        [StringLength(20)]
        public string? ATIN { get; set; }

        // SECTION C: Tax Information for Other Countries
        [Column("country1_tax_jurisdiction", TypeName = "VARCHAR2(100 BYTE)")]
        [StringLength(100)]
        public string? Country1TaxJurisdiction { get; set; }

        [Column("country1_tin", TypeName = "VARCHAR2(50 BYTE)")]
        [StringLength(50)]
        public string? Country1TIN { get; set; }

        [Column("country1_no_tin_reason", TypeName = "VARCHAR2(1 BYTE)")]
        [StringLength(1)]
        public string? Country1NoTinReason { get; set; } // 'A', 'B', or 'C'

        [Column("country1_no_tin_explanation", TypeName = "VARCHAR2(500 BYTE)")]
        [StringLength(500)]
        public string? Country1NoTinExplanation { get; set; }

        [Column("country2_tax_jurisdiction", TypeName = "VARCHAR2(100 BYTE)")]
        [StringLength(100)]
        public string? Country2TaxJurisdiction { get; set; }

        [Column("country2_tin", TypeName = "VARCHAR2(50 BYTE)")]
        [StringLength(50)]
        public string? Country2TIN { get; set; }

        [Column("country2_no_tin_reason", TypeName = "VARCHAR2(1 BYTE)")]
        [StringLength(1)]
        public string? Country2NoTinReason { get; set; } // 'A', 'B', or 'C'

        [Column("country2_no_tin_explanation", TypeName = "VARCHAR2(500 BYTE)")]
        [StringLength(500)]
        public string? Country2NoTinExplanation { get; set; }

        [Column("country3_tax_jurisdiction", TypeName = "VARCHAR2(100 BYTE)")]
        [StringLength(100)]
        public string? Country3TaxJurisdiction { get; set; }

        [Column("country3_tin", TypeName = "VARCHAR2(50 BYTE)")]
        [StringLength(50)]
        public string? Country3TIN { get; set; }

        [Column("country3_no_tin_reason", TypeName = "VARCHAR2(1 BYTE)")]
        [StringLength(1)]
        public string? Country3NoTinReason { get; set; } // 'A', 'B', or 'C'

        [Column("country3_no_tin_explanation", TypeName = "VARCHAR2(500 BYTE)")]
        [StringLength(500)]
        public string? Country3NoTinExplanation { get; set; }

        [Column("created_at", TypeName = "TIMESTAMP(6)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at", TypeName = "TIMESTAMP(6)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("CustomerId")]
        public virtual CustMast Customer { get; set; } = null!;
    }
}
