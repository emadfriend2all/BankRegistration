using Microsoft.AspNetCore.Mvc;
using RegistrationPortal.Server.Services;
using RegistrationPortal.Server.DTOs;

namespace RegistrationPortal.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerDocumentController : ControllerBase
    {
        private readonly ICustomerDocumentService _documentService;

        public CustomerDocumentController(ICustomerDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("upload/{customerId}/{documentType}/{branchCode}/{idNo2}")]
        public async Task<ActionResult<CustomerDocumentDto>> UploadDocument(string customerId, string documentType, string branchCode, string idNo2, IFormFile file)
        {
            try
            {
                var document = await _documentService.UploadDocumentAsync(customerId, documentType, file, branchCode, idNo2);
                return Ok(new CustomerDocumentDto
                {
                    Id = document.Id,
                    CustomerId = document.CustomerId,
                    DocumentType = document.DocumentType,
                    FileUrl = document.FileUrl,
                    OriginalFileName = document.OriginalFileName,
                    FileSize = document.FileSize,
                    MimeType = document.MimeType,
                    CreatedAt = document.CreatedAt,
                    UpdatedAt = document.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("upload-multiple/{customerId}/{branchCode}/{idNo2}")]
        public async Task<ActionResult<List<CustomerDocumentDto>>> UploadMultipleDocuments(string customerId, string branchCode, string idNo2, [FromForm] UploadCustomerDocumentsDto documents)
        {
            try
            {
                // Convert UploadCustomerDocumentsDto to CreateCustomerDto for processing
                var customerDto = new CreateCustomerDto
                {
                    BranchCCode = branchCode,
                    Identification = documents.Identification,
                    NationalId = documents.NationalId,
                    PersonalImage = documents.PersonalImage,
                    ImageFortheRequesterHoldingTheID = documents.ImageFortheRequesterHoldingTheID,
                    SignitureTemplate = documents.SignitureTemplate,
                    HandwrittenRequest = documents.HandwrittenRequest,
                    EmploymentCertificate = documents.EmploymentCertificate
                };

                var uploadedDocuments = await _documentService.ProcessCustomerDocumentsAsync(customerId, branchCode, idNo2, customerDto);
                
                var result = uploadedDocuments.Select(d => new CustomerDocumentDto
                {
                    Id = d.Id,
                    CustomerId = d.CustomerId,
                    DocumentType = d.DocumentType,
                    FileUrl = d.FileUrl,
                    OriginalFileName = d.OriginalFileName,
                    FileSize = d.FileSize,
                    MimeType = d.MimeType,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<CustomerDocumentDto>>> GetCustomerDocuments(string customerId)
        {
            try
            {
                var documents = await _documentService.GetCustomerDocumentsAsync(customerId);
                var result = documents.Select(d => new CustomerDocumentDto
                {
                    Id = d.Id,
                    CustomerId = d.CustomerId,
                    DocumentType = d.DocumentType,
                    FileUrl = d.FileUrl,
                    OriginalFileName = d.OriginalFileName,
                    FileSize = d.FileSize,
                    MimeType = d.MimeType,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDocumentDto>> GetDocument(string id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                    return NotFound();

                return Ok(new CustomerDocumentDto
                {
                    Id = document.Id,
                    CustomerId = document.CustomerId,
                    DocumentType = document.DocumentType,
                    FileUrl = document.FileUrl,
                    OriginalFileName = document.OriginalFileName,
                    FileSize = document.FileSize,
                    MimeType = document.MimeType,
                    CreatedAt = document.CreatedAt,
                    UpdatedAt = document.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteDocument(string id)
        {
            try
            {
                var result = await _documentService.DeleteDocumentAsync(id);
                if (!result)
                    return NotFound();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadDocument(string id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                    return NotFound();

                if (!System.IO.File.Exists(document.FileUrl))
                    return NotFound("File not found on server");

                var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FileUrl);
                return File(fileBytes, document.MimeType ?? "application/octet-stream", document.OriginalFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
