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
                FileUrl = $"/api/CustomerDocument/download/{documentId}",
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
            var documents = await _documentRepository.GetByCustomerIdAsync(customerId);
            var result = new List<CustomerDocument>();
            
            foreach (var document in documents)
            {
                // Convert download URLs to absolute FilesController URLs for existing documents
                if (document.FileUrl.StartsWith("/api/CustomerDocument/download/"))
                {
                    var documentId = document.FileUrl.Replace("/api/CustomerDocument/download/", "");
                    var physicalPath = FindPhysicalFilePathByOriginalName(document);
                    
                    if (!string.IsNullOrEmpty(physicalPath))
                    {
                        var relativePath = Path.GetRelativePath(_baseStoragePath, physicalPath);
                        // Use absolute URL that works with direct API calls
                        document.FileUrl = $"https://localhost:7232/api/Files/{relativePath.Replace("\\", "/")}";
                        
                        // Update the original file name in the database to match the actual file
                        var actualFileName = Path.GetFileName(physicalPath);
                        if (actualFileName != document.OriginalFileName)
                        {
                            document.OriginalFileName = actualFileName;
                            await _documentRepository.UpdateAsync(document);
                            Console.WriteLine($"Updated original file name for {document.DocumentType} from {document.OriginalFileName} to {actualFileName}");
                        }
                    }
                }
                result.Add(document);
            }
            
            // Save changes if any original file names were updated
            await _documentRepository.SaveChangesAsync();
            
            return result;
        }

        public async Task<string?> GetDocumentFilePathAsync(string documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                return null;

            // Check if the FileUrl is already a web URL (new format)
            if (document.FileUrl.StartsWith("/api/"))
            {
                // For new documents, we need to find the actual file path
                return FindPhysicalFilePath(document);
            }
            else
            {
                // For old documents, the FileUrl contains the local file path
                return document.FileUrl;
            }
        }

        private string? FindPhysicalFilePathByOriginalName(CustomerDocument document)
        {
            Console.WriteLine($"FindPhysicalFilePathByOriginalName: Searching for {document.DocumentType} with original file name: {document.OriginalFileName}");
            
            // Look for files using the original file name instead of document type
            if (string.IsNullOrEmpty(document.OriginalFileName))
            {
                Console.WriteLine($"FindPhysicalFilePathByOriginalName: No original file name, falling back to document type search");
                return FindPhysicalFilePath(document);
            }
            
            // Try to get customer information to determine the exact folder structure
            if (document.CustomerId.Length >= 6)
            {
                var branchCode = document.CustomerId.Substring(0, 3);
                var customerFolder = Path.Combine(_baseStoragePath, branchCode);
                Console.WriteLine($"FindPhysicalFilePathByOriginalName: Looking in customer folder: {customerFolder}");
                
                if (Directory.Exists(customerFolder))
                {
                    var idFolders = Directory.GetDirectories(customerFolder);
                    Console.WriteLine($"FindPhysicalFilePathByOriginalName: Found {idFolders.Length} ID folders");
                    
                    foreach (var idFolder in idFolders)
                    {
                        try
                        {
                            Console.WriteLine($"FindPhysicalFilePathByOriginalName: Checking folder: {idFolder}");
                            
                            // Look for the exact original file name
                            var exactFile = Path.Combine(idFolder, document.OriginalFileName);
                            Console.WriteLine($"FindPhysicalFilePathByOriginalName: Checking exact file: {exactFile}");
                            if (File.Exists(exactFile))
                            {
                                Console.WriteLine($"FindPhysicalFilePathByOriginalName: Found exact file: {exactFile}");
                                return exactFile;
                            }
                            
                            // Look for files with similar names (case insensitive)
                            var allFiles = Directory.GetFiles(idFolder, "*.*")
                                .Where(f => Path.GetFileName(f).Equals(document.OriginalFileName, StringComparison.OrdinalIgnoreCase))
                                .FirstOrDefault();
                            if (!string.IsNullOrEmpty(allFiles))
                            {
                                Console.WriteLine($"FindPhysicalFilePathByOriginalName: Found case-insensitive match: {allFiles}");
                                return allFiles;
                            }
                            
                            // List all files in this folder for debugging
                            var filesInFolder = Directory.GetFiles(idFolder);
                            Console.WriteLine($"FindPhysicalFilePathByOriginalName: Files in folder {idFolder}: {string.Join(", ", filesInFolder.Select(Path.GetFileName))}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"FindPhysicalFilePathByOriginalName: Error checking folder {idFolder}: {ex.Message}");
                            // Continue to next folder
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"FindPhysicalFilePathByOriginalName: Customer folder does not exist: {customerFolder}");
                }
            }
            
            // Fallback: search all directories
            if (Directory.Exists(_baseStoragePath))
            {
                try
                {
                    var allFiles = Directory.GetFiles(_baseStoragePath, document.OriginalFileName, SearchOption.AllDirectories)
                        .FirstOrDefault();
                    if (!string.IsNullOrEmpty(allFiles))
                    {
                        Console.WriteLine($"FindPhysicalFilePathByOriginalName: Found file in global search: {allFiles}");
                        return allFiles;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FindPhysicalFilePathByOriginalName: Error in global search: {ex.Message}");
                }
            }
            
            Console.WriteLine($"FindPhysicalFilePathByOriginalName: No file found, falling back to document type search");
            // Final fallback to the old method
            return FindPhysicalFilePath(document);
        }

        private string? FindPhysicalFilePath(CustomerDocument document)
        {
            // For new documents, we need to find the actual file path
            // The folder structure should be: Files/BranchCode/idNo2
            
            // Try to get customer information to determine the exact folder structure
            // First, try to extract from customer ID pattern (branchCode + custINo)
            if (document.CustomerId.Length >= 6)
            {
                var branchCode = document.CustomerId.Substring(0, 3);
                var customerFolder = Path.Combine(_baseStoragePath, branchCode);
                
                if (Directory.Exists(customerFolder))
                {
                    var idFolders = Directory.GetDirectories(customerFolder);
                    foreach (var idFolder in idFolders)
                    {
                        // Look for files that match the document type pattern
                        // Files could be named like: PersonalImage.jpg, PersonalImage_20241224123456.jpg, etc.
                        
                        // Try exact match first
                        var extensions = new[] { ".png", ".jpg", ".jpeg", ".pdf", ".doc", ".docx" };
                        foreach (var extension in extensions)
                        {
                            var exactFile = Path.Combine(idFolder, $"{document.DocumentType}{extension}");
                            if (File.Exists(exactFile))
                                return exactFile;
                        }
                        
                        // Then try pattern match (files that contain the document type)
                        try
                        {
                            var allFiles = Directory.GetFiles(idFolder, $"*{document.DocumentType}*.*")
                                .OrderByDescending(f => f) // Most recent first
                                .FirstOrDefault();
                            if (!string.IsNullOrEmpty(allFiles))
                                return allFiles;
                        }
                        catch
                        {
                            // Continue to fallback
                        }
                    }
                }
            }
            
            // Fallback: search all directories if the structured approach fails
            if (Directory.Exists(_baseStoragePath))
            {
                var branchFolders = Directory.GetDirectories(_baseStoragePath);
                foreach (var branchFolder in branchFolders)
                {
                    var customerFolders = Directory.GetDirectories(branchFolder);
                    foreach (var customerFolder in customerFolders)
                    {
                        try
                        {
                            var allFiles = Directory.GetFiles(customerFolder, $"*{document.DocumentType}*.*")
                                .OrderByDescending(f => f)
                                .FirstOrDefault();
                            if (!string.IsNullOrEmpty(allFiles))
                                return allFiles;
                        }
                        catch
                        {
                            // Continue
                        }
                    }
                }
            }
            
            return null;
        }

        public async Task<bool> UpdateDocumentUrlsToWebFormat()
        {
            try
            {
                var allDocuments = await _documentRepository.GetAllAsync();
                var updated = false;

                foreach (var document in allDocuments)
                {
                    // Only update documents that still have local file paths
                    if (!document.FileUrl.StartsWith("/api/"))
                    {
                        // Convert local path to web URL
                        if (File.Exists(document.FileUrl))
                        {
                            var fileInfo = new FileInfo(document.FileUrl);
                            var relativePath = Path.GetRelativePath(_baseStoragePath, document.FileUrl);
                            document.FileUrl = $"https://localhost:7232/api/Files/{relativePath.Replace("\\", "/")}";
                            await _documentRepository.UpdateAsync(document);
                            updated = true;
                        }
                    }
                }

                if (updated)
                {
                    await _documentRepository.SaveChangesAsync();
                }

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document URLs to web format");
                return false;
            }
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
                            FileUrl = $"/api/CustomerDocument/download/{documentId}",
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
