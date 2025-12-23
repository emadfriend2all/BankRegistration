using Microsoft.Extensions.Logging;
using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.DTOs;
using RegistrationPortal.Server.Repositories;

namespace RegistrationPortal.Server.Services
{
    public class CustomerDocumentService : ICustomerDocumentService
    {
        private readonly ICustomerDocumentRepository _documentRepository;
        private readonly ILogger<CustomerDocumentService> _logger;
        private readonly string _baseStoragePath;

        public CustomerDocumentService(ICustomerDocumentRepository documentRepository, ILogger<CustomerDocumentService> logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
            _baseStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "Files");
        }

        public async Task<CustomerDocument> UploadDocumentAsync(string customerId, string documentType, IFormFile file, string branchCode, string idNo2)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            // Get customer to extract ID number for folder structure
            var customerDocuments = await _documentRepository.GetByCustomerIdAsync(customerId);
            var existingDocument = customerDocuments.FirstOrDefault(d => d.DocumentType == documentType);
            
            // Generate unique document ID
            var documentId = $"{customerId}_{documentType}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Create folder structure: Files/BranchCode/idNo2
            var customerFolder = Path.Combine(_baseStoragePath, branchCode.Replace("/", "_"), idNo2.Replace("/", "_"));
            
            if (!Directory.Exists(customerFolder))
            {
                Directory.CreateDirectory(customerFolder);
            }

            // Generate file name
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{documentType}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(customerFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create document record
            var document = new CustomerDocument
            {
                Id = documentId,
                CustomerId = customerId,
                DocumentType = documentType,
                FileUrl = filePath,
                OriginalFileName = file.FileName,
                FileSize = file.Length,
                MimeType = file.ContentType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentType} uploaded successfully for customer {CustomerId}", documentType, customerId);

            return document;
        }

        public async Task<IEnumerable<CustomerDocument>> GetCustomerDocumentsAsync(string customerId)
        {
            return await _documentRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<CustomerDocument?> GetDocumentByIdAsync(string id)
        {
            return await _documentRepository.GetByIdAsync(id);
        }

        public async Task<bool> DeleteDocumentAsync(string id)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
                return false;

            // Delete file from storage
            if (File.Exists(document.FileUrl))
            {
                File.Delete(document.FileUrl);
            }

            // Delete from database
            await _documentRepository.DeleteAsync(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} deleted successfully", id);
            return true;
        }

        public async Task<List<CustomerDocument>> ProcessCustomerDocumentsAsync(string customerId, string branchCode, string idNo2, CreateCustomerDto customerDto)
        {
            var uploadedDocuments = new List<CustomerDocument>();
            
            // Create folder structure: Files/BranchCode/idNo2
            var customerFolder = Path.Combine(_baseStoragePath, branchCode.Replace("/", "_"), idNo2.Replace("/", "_"));
            
            if (!Directory.Exists(customerFolder))
            {
                Directory.CreateDirectory(customerFolder);
            }

            var documentProperties = new[]
            {
                new { File = customerDto.Identification, Type = DocumentTypes.Identification },
                new { File = customerDto.NationalId, Type = DocumentTypes.NationalId },
                new { File = customerDto.PersonalImage, Type = DocumentTypes.PersonalImage },
                new { File = customerDto.ImageFortheRequesterHoldingTheID, Type = DocumentTypes.ImageFortheRequesterHoldingTheID },
                new { File = customerDto.SignitureTemplate, Type = DocumentTypes.SignitureTemplate },
                new { File = customerDto.HandwrittenRequest, Type = DocumentTypes.HandwrittenRequest },
                new { File = customerDto.EmploymentCertificate, Type = DocumentTypes.EmploymentCertificate }
            };

            foreach (var docProp in documentProperties)
            {
                if (docProp.File != null && docProp.File.Length > 0)
                {
                    try
                    {
                        var documentId = Guid.NewGuid().ToString();
                        var fileExtension = Path.GetExtension(docProp.File.FileName);
                        var fileName = $"{docProp.Type}{fileExtension}";
                        var filePath = Path.Combine(customerFolder, fileName);

                        // Save file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await docProp.File.CopyToAsync(stream);
                        }

                        // Create document record
                        var document = new CustomerDocument
                        {
                            Id = documentId,
                            CustomerId = customerId,
                            DocumentType = docProp.Type,
                            FileUrl = filePath,
                            OriginalFileName = docProp.File.FileName,
                            FileSize = docProp.File.Length,
                            MimeType = docProp.File.ContentType,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _documentRepository.AddAsync(document);
                        uploadedDocuments.Add(document);

                        _logger.LogInformation("Document {DocumentType} uploaded for customer {CustomerId}", docProp.Type, customerId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading document {DocumentType} for customer {CustomerId}", docProp.Type, customerId);
                        // Continue with other documents even if one fails
                    }
                }
            }

            if (uploadedDocuments.Any())
            {
                await _documentRepository.SaveChangesAsync();
                _logger.LogInformation("Successfully uploaded {Count} documents for customer {CustomerId}", uploadedDocuments.Count, customerId);
            }

            return uploadedDocuments;
        }
    }
}
