using System.ComponentModel.DataAnnotations;

namespace RegistrationPortal.Server.DTOs
{
    public class UpdateCustomerReviewDto
    {
        [Required]
        [StringLength(50)]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ReviewStatus { get; set; } = string.Empty;
    }
}
