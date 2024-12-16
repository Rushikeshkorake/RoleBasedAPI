using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RoleBasedAPI.Data;
using RoleBasedAPI.Model;
using Sieve.Models;
using Sieve.Services;
using System.Security.Claims;

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

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost("UploadDocument")]
        public async Task<IActionResult> UploadDocument(IFormFile file, [FromForm] string title, [FromForm] string tags)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            var allowedExtensions = new[] { ".pdf", ".docx", ".txt" };

            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Invalid file type. Only PDF, Word, and Text files are allowed.");

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine("UploadedFiles", fileName);

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            
            var document = new Document
            {
                Title = title,
                Tags = tags,
                FilePath = filePath,
                UploadedBy = userId,
                UploadedDate = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Document uploaded successfully.", documentId = document.Id });
        }


        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchDocuments(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Search term cannot be empty.");
            }

            searchTerm = searchTerm.Replace(" ", " & "); 

            var documents = await _context.Documents
                .FromSqlRaw(
                    @"SELECT * FROM ""Documents"" 
              WHERE SearchVector @@ to_tsquery('english', {0})",
                    searchTerm)
                .ToListAsync();

            return Ok(documents);
        }

        [Authorize(Policy = "AdminPolicy")]

        [HttpGet("Filtering")]
        public async Task<IActionResult> FilterDocuments(string searchTerm, string uploadedBy, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Search term cannot be empty.");
            }

            searchTerm = searchTerm.Replace(" ", " & ");

            var query = _context.Documents
                .FromSqlRaw(
                    @"SELECT * FROM ""Documents"" 
              WHERE SearchVector @@ to_tsquery('english', @searchTerm)",
                    new NpgsqlParameter("@searchTerm", searchTerm))
                .AsQueryable();

            if (!string.IsNullOrEmpty(uploadedBy))
            {
                query = query.Where(d => d.UploadedBy == uploadedBy);
            }

            if (startDate.HasValue)
            {
                //query = query.Where(d => d.UploadedDate >= startDate.Value);
                query = query.Where(d => d.UploadedDate >= DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc));
            }

            if (endDate.HasValue)
            {
                //query = query.Where(d => d.UploadedDate <= endDate.Value);
                query = query.Where(d => d.UploadedDate <= DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc));
            }

            var documents = await query.ToListAsync();

            return Ok(documents);
        }

        // Admins, Contributors, and Viewers can view documents.
        [Authorize(Policy = "AllRolesPolicy")]
        [HttpGet("GetDocument/{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                return NotFound(new { message = "Document not found." });
            }

            return Ok(document);
        }
    }
}
