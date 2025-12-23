using System.ComponentModel.DataAnnotations;

namespace RegistrationPortal.Server.DTOs
{
    public class CreateCustomerDto
    {
        [Required]
        public string BranchCCode { get; set; } = string.Empty;

        [Required]
        public string CustCName { get; set; } = string.Empty;

        public string CustCFname { get; set; } = string.Empty;

        public string CustCSname { get; set; } = string.Empty;

        public string CustCTname { get; set; } = string.Empty;

        public string CustCFoname { get; set; } = string.Empty;

        public string CustCMname { get; set; } = string.Empty;

        public string? CustCSex { get; set; }

        public string? CustCReligion { get; set; }

        public string? CustCCaste { get; set; }

        public string? CustCMaritalsts { get; set; }

        public string? CustCPadd1 { get; set; }

        public string? CustCPadd2 { get; set; }

        public string? CustCPadd3 { get; set; }

        public string? CustCPCity { get; set; }

        public string? CustCPstate { get; set; }

        public string? MobileCNo { get; set; }

        public string? EmailCAdd { get; set; }

        public string? IdCType { get; set; }

        public string? IdCNo { get; set; }

        public string? IdCIssplace { get; set; }

        public DateTime? IdDIssdate { get; set; }

        public DateTime? IdDExpdate { get; set; }

        public string? CustCAuthority { get; set; }

        public string? HusbCName { get; set; }

        public string? CountryCCode { get; set; }

        public string? PlaceCBirth { get; set; }

        public string? CustCNationality { get; set; }

        public string? CustCWife1 { get; set; }

        public string? IdCType2 { get; set; }

        public string? IdCNo2 { get; set; }

        public string? IdCIssplace2 { get; set; }

        public string? IdCIssueDate2 { get; set; }

        public string? IdCExpiryDate2 { get; set; }
        public DateTime? CustDBdate { get; set; }

        public string? IdCAuthority2 { get; set; }

        public string? CustCOccupation { get; set; }

        public decimal? HomeINumber { get; set; }

        public string? CustIIdentify { get; set; }

        public string? CustCCountrybrith { get; set; }

        public string? CustCStatebrith { get; set; }

        public string? CustCCitizenship { get; set; }

        public string? CustCEmployer { get; set; }

        public decimal? CustFIncome { get; set; }

        public string? CustCHigheducation { get; set; }

        public decimal? CustFAvgmonth { get; set; }

        public string? TradeCNameenglish { get; set; }

        public string? CustCEngfname { get; set; }

        public string? CustCEngsname { get; set; }

        public string? CustCEngtname { get; set; }

        public string? CustCEngfoname { get; set; }

        public string? Status { get; set; }

        public DateTime CustDEntrydt { get; set; }

        public List<CreateAccountDto>? AccountMasts { get; set; }

        // FATCA Properties
        public string? IsUsPerson { get; set; } // 'Y' or 'N'
        public string? CitizenshipCountries { get; set; }
        public string? HasImmigrantVisa { get; set; } // 'Y' or 'N'
        public string? PermanentResidencyStates { get; set; }
        public string? HasOtherCountriesResidency { get; set; } // 'Y' or 'N'
        public string? SoleSudanResidencyConfirmed { get; set; } // 'Y' or 'N'
        public string? SSN { get; set; }
        public string? ITIN { get; set; }
        public string? ATIN { get; set; }
        public string? Country1TaxJurisdiction { get; set; }
        public string? Country1TIN { get; set; }
        public string? Country1NoTinReason { get; set; } // 'A', 'B', or 'C'
        public string? Country1NoTinExplanation { get; set; }
        public string? Country2TaxJurisdiction { get; set; }
        public string? Country2TIN { get; set; }
        public string? Country2NoTinReason { get; set; } // 'A', 'B', or 'C'
        public string? Country2NoTinExplanation { get; set; }
        public string? Country3TaxJurisdiction { get; set; }
        public string? Country3TIN { get; set; }
        public string? Country3NoTinReason { get; set; } // 'A', 'B', or 'C'
        public string? Country3NoTinExplanation { get; set; }

        // Document Attachments
        public IFormFile? Identification { get; set; }
        public IFormFile? NationalId { get; set; }
        public IFormFile? PersonalImage { get; set; }
        public IFormFile? ImageFortheRequesterHoldingTheID { get; set; }
        public IFormFile? SignitureTemplate { get; set; }
        public IFormFile? HandwrittenRequest { get; set; }
        public IFormFile? EmploymentCertificate { get; set; }
    }
}
