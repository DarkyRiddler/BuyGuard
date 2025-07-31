using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.DTOs.Request;
using server.Data;
using server.Models;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
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
    /// <summary>
    /// Dodaje załącznik do zgłoszenia.
    /// </summary>
    /// <param name="requestId">Identyfikator zgłoszenia</param>
    /// <param name="file">Plik do załączenia</param>
    /// <response code="200">Załącznik został dodany</response>
    /// <response code="400">Błąd walidacji pliku</response>
    /// <response code="401">Brak autoryzacji</response>
    /// <response code="403">Brak uprawnień</response>
    /// <response code="404">Nie znaleziono zgłoszenia</response>
    [SwaggerOperation(Summary = "Dodaje załącznik do zgłoszenia", Description = "Wymaga roli 'employee' oraz bycie właścicielem zgłoszenia.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    
 
    /// <summary>
    /// Pobiera listę załączników powiązanych ze zgłoszeniem.
    /// </summary>
    /// <param name="requestId">Identyfikator zgłoszenia</param>
    /// <response code="200">Zwraca listę załączników</response>
    /// <response code="401">Brak autoryzacji</response>
    /// <response code="403">Brak dostępu</response>
    /// <response code="404">Nie znaleziono zgłoszenia</response>
    [SwaggerOperation(Summary = "Pobiera listę załączników", Description = "Zwraca listę załączników dostępnych dla właściciela, managera lub admina.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// Usuwa załącznik.
    /// </summary>
    /// <param name="attachmentId">Identyfikator załącznika</param>
    /// <response code="200">Załącznik został usunięty</response>
    /// <response code="400">Brak powiązania załącznika</response>
    /// <response code="401">Brak autoryzacji</response>
    /// <response code="403">Brak dostępu</response>
    /// <response code="404">Załącznik lub plik nie istnieje</response>
    [SwaggerOperation(Summary = "Usuwa załącznik", Description = "Pozwala usunąć plik, jeśli użytkownik jest adminem lub właścicielem zgłoszenia.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{attachmentId}")]
    public async Task<IActionResult> DeleteAttachment(int attachmentId)
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
            return BadRequest("Załącznik niepowiązany ze zgłoszeniem");

        if (userRole != "admin" && attachment.Request.UserId != clientId && attachment.Request.ManagerId != clientId)
            return Forbid("Brak dostępu do usunięcia pliku");

        var uploadsFolder = Path.Combine("uploads", "attachments");
        var fileName = Path.GetFileName(attachment.FileUrl);
        var filePath = Path.Combine(uploadsFolder, fileName);

        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
        else
            return NotFound("Plik nie istnieje fizycznie na serwerze");
        _db.Attachment.Remove(attachment);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Załącznik usunięty" });
    }

    /// <summary>
    /// Pobiera plik załącznika powiązanego ze zgłoszeniem.
    /// </summary>
    /// <param name="attachmentId">Identyfikator załącznika</param>
    /// <response code="200">Zwraca plik jako strumień</response>
    /// <response code="401">Brak autoryzacji</response>
    /// <response code="403">Brak dostępu do pliku</response>
    /// <response code="404">Załącznik nie istnieje lub brak pliku fizycznego</response>
    [SwaggerOperation(Summary = "Pobiera zawartość pliku załącznika", Description = "Zwraca binarny plik jako strumień danych.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
