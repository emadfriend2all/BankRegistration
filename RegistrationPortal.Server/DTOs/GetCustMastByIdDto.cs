using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationPortal.Server.DTOs
{
    public class GetCustMastByIdDto
    {
        [Required]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string BranchCCode { get; set; } = string.Empty;

        public decimal? CustINo { get; set; }

        [Required]
        [StringLength(250)]
        public string CustCName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CustCFname { get; set; }

        [StringLength(100)]
        public string? CustCSname { get; set; }

        [StringLength(100)]
        public string? CustCTname { get; set; }

        [StringLength(100)]
        public string? CustCFoname { get; set; }

        [StringLength(250)]
        public string? CustCMname { get; set; }

        [Column(TypeName = "date")]
        public DateTime? CustDBdate { get; set; }

        [StringLength(10)]
        public string? CustCSex { get; set; }

        [StringLength(50)]
        public string? CustCReligion { get; set; }

        [StringLength(50)]
        public string? CustCCaste { get; set; }

        [StringLength(50)]
        public string? CustCMaritalsts { get; set; }

        [StringLength(500)]
        public string? CustCPadd1 { get; set; }

        [StringLength(100)]
        public string? CustCPCity { get; set; }

        [StringLength(50)]
        public string? MobileCNo { get; set; }

        [StringLength(250)]
        public string? EmailCAdd { get; set; }

        [StringLength(50)]
        public string? IdCType { get; set; }

        [StringLength(100)]
        public string? IdCNo { get; set; }

        [Column(TypeName = "date")]
        public DateTime? IdDIssdate { get; set; }

        [StringLength(100)]
        public string? IdCIssplace { get; set; }

        [Column(TypeName = "date")]
        public DateTime? IdDExpdate { get; set; }

        [StringLength(100)]
        public string? CustCAuthority { get; set; }

        [StringLength(250)]
        public string? HusbCName { get; set; }

        [StringLength(10)]
        public string? CountryCCode { get; set; }

        [StringLength(100)]
        public string? PlaceCBirth { get; set; }

        [StringLength(100)]
        public string? CustCNationality { get; set; }

        [StringLength(250)]
        public string? CustCWife1 { get; set; }

        [StringLength(50)]
        public string? IdCType2 { get; set; }

        [StringLength(100)]
        public string? IdCNo2 { get; set; }

        [StringLength(100)]
        public string? CustCOccupation { get; set; }

        [StringLength(50)]
        public string? HomeINumber { get; set; }

        [StringLength(100)]
        public string? CustIIdentify { get; set; }

        [StringLength(100)]
        public string? CustCCountrybrith { get; set; }

        [StringLength(100)]
        public string? CustCStatebrith { get; set; }

        [StringLength(100)]
        public string? CustCCitizenship { get; set; }

        [StringLength(250)]
        public string? CustCEmployer { get; set; }

        public decimal? CustFIncome { get; set; }

        [StringLength(100)]
        public string? CustCHigheducation { get; set; }

        public decimal? CustFAvgmonth { get; set; }

        [StringLength(250)]
        public string? TradeCNameenglish { get; set; }

        [StringLength(100)]
        public string? CustCEngfname { get; set; }

        [StringLength(100)]
        public string? CustCEngsname { get; set; }

        [StringLength(100)]
        public string? CustCEngtname { get; set; }

        [StringLength(100)]
        public string? CustCEngfoname { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(50)]
        public string? ReviewStatus { get; set; }

        [StringLength(100)]
        public string? ReviewedBy { get; set; }

        public DateTime CustDEntrydt { get; set; }

        // Navigation properties for related data
        public List<AccountMastDto> AccountMasts { get; set; } = new List<AccountMastDto>();
        public List<CustomerDocumentDto> CustomerDocuments { get; set; } = new List<CustomerDocumentDto>();
    }
}
