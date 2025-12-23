using System.ComponentModel.DataAnnotations;

namespace RegistrationPortal.Server.DTOs
{
    public class CreateAccountDto
    {
        [Required]
        public string BranchCCode { get; set; } = string.Empty;

        [Required]
        public string ActCType { get; set; } = string.Empty;

        public string? CurrencyCCode { get; set; }

        public string? ActCName { get; set; }

        public string? ActCOcccode { get; set; }

        public DateTime? ActDOpdate { get; set; }

        public string? ActCOrgn { get; set; }

        public string? ActCActsts { get; set; }

        public string? ActCOdsts { get; set; }

        public string? ActCChqfac { get; set; }

        public string? ActCIntrotype { get; set; }

        public string? ActCOpmode { get; set; }

        public decimal? ActIIntroid { get; set; }

        public decimal? ActITrbrcode { get; set; }

        public DateTime? ActDClosdt { get; set; }

        public string? ActCAbbsts { get; set; }

        public string? WithdrawCFlag { get; set; }

        public string? IntroCRem { get; set; }

        public string? ActCAtm { get; set; }

        public string? ActCInternet { get; set; }

        public string? ActCTelebnk { get; set; }

        public string? GroupCCode { get; set; }

        public string? EnteredCBy { get; set; }

        public string? AuthCBy { get; set; }

        public string? CloseCReason { get; set; }
    }
}
