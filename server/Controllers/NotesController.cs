using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using server.DTOs.Note;
using server.Data;
using server.Models;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public NotesController(ApplicationDbContext db)
    {
        this._db = db;
    }

    [HttpPatch("requests/{requestId}")]
    public IActionResult UpdateRequestNote(int requestId, [FromBody] UpdateNoteRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        var requestEntity = _db.Request
            .Include(r => r.Notes)
            .FirstOrDefault(r => r.Id == requestId);
        if (requestEntity == null)
            return NotFound("Zgłoszenie nie istnieje");
        if (userRole != "admin" && requestEntity.UserId != userId && requestEntity.ManagerId != userId)
            return Forbid("Brak uprawnień do edycji notatki zgłoszenia");
        var existingNote = requestEntity.Notes?.FirstOrDefault();
        if (existingNote != null)
        {
            existingNote.Body = request.Body;
            existingNote.AuthorId = userId;
        }
        else
        {
            var newNote = new Note
            {
                RequestId = requestId,
                AuthorId = userId,
                Body = request.Body,
                CreatedAt = DateTime.UtcNow
            };
            _db.Note.Add(newNote);
        }
        _db.SaveChanges();
        return Ok(new
        {
            success = true,
            message = "Zaktualizowano notsa"
        });
    }

    [HttpGet("requests/{requestId}")]
    public IActionResult GetRequestNote(int requestId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        var requestEntity = _db.Request
            .Include(r => r.Notes)
            .ThenInclude(n => n.Author)
            .FirstOrDefault(r => r.Id == requestId);
        if (requestEntity == null) return NotFound("Zgłoszenie nie istnieje");
        if (userRole != "admin" && requestEntity.UserId != userId && requestEntity.ManagerId != userId)
            return Forbid("Brak uprawnien!!!");
        var note = requestEntity.Notes?.FirstOrDefault();
        if (note == null)
            return NotFound("Notatka nie istnieje");
        return Ok(new
        {
            id = note.Id,
            body = note.Body,
            createdAt = note.CreatedAt,
            author = new
            {
                id = note.AuthorId,
                name = $"{note.Author?.FirstName} {note.Author?.LastName}",
                email = note.Author?.Email
            }
        });
    }
}