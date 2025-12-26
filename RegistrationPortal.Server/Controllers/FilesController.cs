using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace RegistrationPortal.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Allow anonymous access for file serving
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _baseStoragePath;

        public FilesController(IWebHostEnvironment environment)
        {
            _environment = environment;
            _baseStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "Files");
        }

        [HttpGet("{*path}")]
        public IActionResult GetFile(string path)
        {
            try
            {
                // Security: Ensure the path doesn't contain directory traversal attacks
                if (path.Contains("..") || path.Contains("//") || path.StartsWith("/"))
                {
                    return BadRequest("Invalid path");
                }

                var fullPath = Path.Combine(_baseStoragePath, path);
                
                // Log for debugging
                Console.WriteLine($"FilesController: Looking for file at: {fullPath}");
                Console.WriteLine($"FilesController: Base path: {_baseStoragePath}");
                Console.WriteLine($"FilesController: Requested path: {path}");
                
                if (!System.IO.File.Exists(fullPath))
                {
                    Console.WriteLine($"FilesController: File not found at: {fullPath}");
                    
                    // Try to find similar files for debugging
                    var directory = Path.GetDirectoryName(fullPath);
                    if (Directory.Exists(directory))
                    {
                        var files = Directory.GetFiles(directory, "*.jpg").Concat(Directory.GetFiles(directory, "*.png")).Concat(Directory.GetFiles(directory, "*.jpeg"));
                        Console.WriteLine($"FilesController: Available files in directory: {string.Join(", ", files.Select(f => Path.GetFileName(f)))}");
                    }
                    
                    return NotFound("File not found");
                }

                // Determine content type
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fullPath, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var fileBytes = System.IO.File.ReadAllBytes(fullPath);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FilesController Error: {ex.Message}");
                return StatusCode(500, $"Error serving file: {ex.Message}");
            }
        }
    }
}
