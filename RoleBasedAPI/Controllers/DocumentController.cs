using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoleBasedAPI.Data;
using RoleBasedAPI.Model;

namespace RoleBasedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentDbContext _context;

        public DocumentController(DocumentDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("UploadDocument")]
        public async Task<IActionResult> UploadDocument(IFormFile file, [FromForm] string title, [FromForm] string tags)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            // Generate a unique file name (to avoid overwriting)
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            // Generate file path
            var filePath = Path.Combine("UploadedFiles", fileName);
            // Ensure the upload directory exists

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            // Save file to server

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save metadata to database
            var document = new Document
            {
                Title = title,
                Tags = tags,
                FilePath = filePath,
                UploadedBy = "TestUser", // Replace with actual user ID if authentication is implemented
                UploadedDate = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Document uploaded successfully.", documentId = document.Id });
        }

        [Authorize(Roles = "Admin,Contributor,Viewer")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchDocuments(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Search term cannot be empty.");
            }

            var query = _context.Documents
                .FromSqlRaw(@"SELECT * FROM ""Documents"" WHERE to_tsvector('english', ""Title"" || ' ' || ""Tags"") @@ to_tsquery('english', {0})", searchTerm);

            var documents = await query.ToListAsync();

            return Ok(documents);
        }

        [Authorize(Roles = "Admin,Contributor,Viewer")]
        [HttpGet("GetDocument/{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            // Admins, Contributors, and Viewers can view documents.
              return Ok( "Get documents");
        }
    }
}
