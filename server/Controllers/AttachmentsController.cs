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
        this._db = db;
    }
    [HttpPost("requests/{requestId}/attachment")]
    public IActionResult UploadAttachment(int requestId, IFormFile file) {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();

        var request = _db.Request
            .Include(r => r.Attachments)
            .FirstOrDefault(r => r.Id == requestId);
        if (request == null)
            return NotFound("Zgłoszenie nie istnieje");

        if (request.UserId != clientId)
            return Forbid("Nie masz uprawnień do dodawania załączników do tego zgłoszenia.");
        
        if (currentUserRole != "employee")
            return Forbid("Tylko pracownicy mogą dodawać załączniki do zgłoszeń.");

        if (file == null || file.Length == 0)
            return BadRequest("Plik jest pusty");
        var allowedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
        
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("Plik przekracza limit 5 MB");
        
        var uploadsFolder = Path.Combine("uploads", "attachments");
        Directory.CreateDirectory(uploadsFolder);
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);

        var attachment = new Attachment
        {
            FileUrl = "/uploads/attachments/" + fileName,
            MimeType = file.ContentType,
            RequestId = requestId
        };

        _db.Attachment.Add(attachment);
        _db.SaveChanges();


        return Ok(new { message = "Załącznik dodany", url = attachment.FileUrl });
    }

    [HttpGet("requests/{requestId}/attachment")]


    [HttpGet("requests/{requestId}")]



}   