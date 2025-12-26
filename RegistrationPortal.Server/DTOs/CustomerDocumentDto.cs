using RegistrationPortal.Server.Entities;

namespace RegistrationPortal.Server.DTOs
{
    public class CustomerDocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? OriginalFileName { get; set; }
        public decimal FileSize { get; set; }
        public string? MimeType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateCustomerDocumentDto
    {
        public required string DocumentType { get; set; }
        public IFormFile? File { get; set; }
    }

    public class UploadCustomerDocumentsDto
    {
        public IFormFile? Identification { get; set; }
        public IFormFile? NationalId { get; set; }
        public IFormFile? PersonalImage { get; set; }
        public IFormFile? ImageFortheRequesterHoldingTheID { get; set; }
        public IFormFile? SignitureTemplate { get; set; }
        public IFormFile? HandwrittenRequest { get; set; }
        public IFormFile? EmploymentCertificate { get; set; }
    }
}
