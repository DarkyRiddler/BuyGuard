using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.DTOs.Request;
using server.Data;
using server.Models;
using Microsoft.EntityFrameworkCore;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttachmentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AttachmentsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost("requests/{requestId}/attachment")]
    public async Task<IActionResult> UploadAttachment(int requestId, IFormFile file)
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();

        var request = await _db.Request
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
            return NotFound("Zgłoszenie nie istnieje");

        if (request.UserId != clientId)
            return Forbid("Nie masz uprawnień do dodawania załączników do tego zgłoszenia.");

        if (currentUserRole != "employee")
            return Forbid("Tylko pracownicy mogą dodawać załączniki do zgłoszeń.");

        if (file == null || file.Length == 0)
            return BadRequest("Plik jest pusty");
        var allowedTypes = new[] { "image/jpeg", "image/png", "application/pdf"};

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("Plik przekracza limit 5 MB");

        var uploadsFolder = Path.Combine("uploads", "attachments");
        Directory.CreateDirectory(uploadsFolder);
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Asynchroniczne zapisywanie pliku
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new Attachment
        {
            FileUrl = "/uploads/attachments/" + fileName,
            MimeType = file.ContentType,
            RequestId = requestId
        };

        await _db.Attachment.AddAsync(attachment);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Załącznik dodany", url = attachment.FileUrl });
    }

    [HttpGet("requests/{requestId}/attachment")]
    public async Task<IActionResult> GetRequestAttachments(int requestId)
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();

        var request = await _db.Request
            .Include(r => r.Attachments)
            .Include(r => r.User)
            .Include(r => r.Manager)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            return NotFound();

        if (userRole != "admin" && request.UserId != clientId && request.ManagerId != clientId)
            return Forbid();

        var attachments = request.Attachments?.Select(a => new
        {
            a.Id,
            a.FileUrl,
            a.MimeType
        }).ToList();

        return Ok(attachments);
    }

    /// <summary>
    /// Pobiera plik załącznika powiązanego ze zgłoszeniem.
    /// </summary>
    /// <param name="attachmentId">Identyfikator załącznika</param>
    /// <returns>Plik binarny jako odpowiedź</returns>
    /// <response code="200">Zwraca plik jako strumień</response>
    /// <response code="401">Brak autoryzacji</response>
    /// <response code="403">Brak dostępu do pliku</response>
    /// <response code="404">Załącznik nie istnieje lub brak pliku fizycznego</response>
    [HttpGet("{attachmentId}/download")]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();

        var attachment = await _db.Attachment
            .Include(a => a.Request)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null)
            return NotFound("Załącznik nie istnieje");

        if (attachment.Request == null)
            return BadRequest("Załącznik niepowiązany z żadnym zgłoszeniem");

        if (userRole != "admin" && attachment.Request.UserId != clientId && attachment.Request.ManagerId != clientId)
            return Forbid("Brak dostępu do tego pliku");

        var uploadsFolder = Path.Combine("uploads", "attachments");
        var fileName = Path.GetFileName(attachment.FileUrl);
        var filePath = Path.Combine(uploadsFolder, fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound("Plik nie istnieje fizycznie na serwerze");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(stream, attachment.MimeType ?? "application/octet-stream");
    }
}
