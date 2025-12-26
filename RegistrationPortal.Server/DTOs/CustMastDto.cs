using System.ComponentModel.DataAnnotations;

namespace RegistrationPortal.Server.DTOs
{
    public class CustMastDto
    {
        [Required]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string BranchCCode { get; set; } = string.Empty;

        public decimal? CustIId { get; set; }

        [Required]
        [StringLength(250)]
        public string CustCName { get; set; } = string.Empty;

        public DateTime? CustDEntrydt { get; set; }

        [StringLength(250)]
        public string? CustCFname { get; set; }

        [StringLength(250)]
        public string? CustCSname { get; set; }

        [StringLength(250)]
        public string? CustCTname { get; set; }

        [StringLength(250)]
        public string? CustCFoname { get; set; }

        [StringLength(250)]
        public string? CustCMname { get; set; }

        public DateTime? CustDBdate { get; set; }

        [StringLength(10)]
        public string? CustCSex { get; set; }

        [StringLength(20)]
        public string? CustCReligion { get; set; }

        [StringLength(50)]
        public string? CustCCaste { get; set; }

        [StringLength(20)]
        public string? CustCMaritalsts { get; set; }

        [StringLength(100)]
        public string? CustCPadd1 { get; set; }

        [StringLength(50)]
        public string? CustCPCity { get; set; }

        [StringLength(18)]
        public string? MobileCNo { get; set; }

        [StringLength(50)]
        public string? EmailCAdd { get; set; }

        [StringLength(50)]
        public string? IdCType { get; set; }

        [StringLength(50)]
        public string? IdCNo { get; set; }

        public DateTime? IdDIssdate { get; set; }

        [StringLength(50)]
        public string? IdCIssplace { get; set; }

        public DateTime? IdDExpdate { get; set; }

        [StringLength(50)]
        public string? CustCAuthority { get; set; }

        [StringLength(100)]
        public string? HusbCName { get; set; }

        [StringLength(50)]
        public string? CountryCCode { get; set; }

        [StringLength(100)]
        public string? PlaceCBirth { get; set; }

        [StringLength(100)]
        public string? CustCNationality { get; set; }

        [StringLength(250)]
        public string? CustCWife1 { get; set; }

        [StringLength(50)]
        public string? IdCType2 { get; set; }

        [StringLength(50)]
        public string? IdCNo2 { get; set; }

        [StringLength(100)]
        public string? CustCOccupation { get; set; }

        public decimal? HomeINumber { get; set; }

        [StringLength(20)]
        public string? CustIIdentify { get; set; }

        [StringLength(50)]
        public string? CustCCountrybrith { get; set; }

        [StringLength(10)]
        public string? CustCStatebrith { get; set; }

        [StringLength(50)]
        public string? CustCCitizenship { get; set; }

        [StringLength(50)]
        public string? CustCEmployer { get; set; }

        public decimal? CustFIncome { get; set; }

        [StringLength(50)]
        public string? CustCHigheducation { get; set; }

        public decimal? CustFAvgmonth { get; set; }

        [StringLength(250)]
        public string? TradeCNameenglish { get; set; }

        [StringLength(250)]
        public string? CustCEngfname { get; set; }

        [StringLength(250)]
        public string? CustCEngsname { get; set; }

        [StringLength(250)]
        public string? CustCEngtname { get; set; }

        [StringLength(250)]
        public string? CustCEngfoname { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(50)]
        public string? ReviewStatus { get; set; }

        [StringLength(100)]
        public string? ReviewedBy { get; set; }

        // Navigation properties as DTOs
        public List<AccountMastDto> AccountMasts { get; set; } = new();
        public List<CustomerDocumentDto> CustomerDocuments { get; set; } = new();
    }

    public class AccountMastDto
    {
        [Required]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string BranchCCode { get; set; } = string.Empty;

        [StringLength(10)]
        public string? ActCType { get; set; }

        public decimal? CustINo { get; set; }

        [StringLength(10)]
        public string? CurrencyCCode { get; set; }

        [StringLength(250)]
        public string? ActCName { get; set; }
    }
}
