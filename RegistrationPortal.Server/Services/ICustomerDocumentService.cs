using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.DTOs;

namespace RegistrationPortal.Server.Services
{
    public interface ICustomerDocumentService
    {
        Task<CustomerDocument> UploadDocumentAsync(string customerId, string documentType, IFormFile file, string branchCode, string idNo2);
        Task<IEnumerable<CustomerDocument>> GetCustomerDocumentsAsync(string customerId);
        Task<string?> GetDocumentFilePathAsync(string documentId);
        Task<CustomerDocument?> GetDocumentByIdAsync(string id);
        Task<bool> DeleteDocumentAsync(string id);
        Task<List<CustomerDocument>> ProcessCustomerDocumentsAsync(string customerId, string branchCode, string idNo2, CreateCustomerDto customerDto);
        Task<bool> UpdateDocumentUrlsToWebFormat();
    }
}
