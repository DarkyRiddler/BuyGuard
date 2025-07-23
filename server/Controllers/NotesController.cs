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

    [HttpPost("requests/{requestId}")]
    public async Task<IActionResult> CreateNoteAsync(int requestId, [FromBody] CreateNoteRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var requestEntity = await _db.Request.FirstOrDefaultAsync(r => r.Id == requestId);
        if (requestEntity == null)
            return NotFound("Zgłoszenie nie istnieje");

        if (userRole != "admin" && requestEntity.UserId != userId && requestEntity.ManagerId != userId)
            return Forbid("Brak uprawnień do dodawania notatek do tego zgłoszenia");

        var newNote = new Note
        {
            RequestId = requestId,
            AuthorId = userId,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };

        await _db.Note.AddAsync(newNote);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Dodano notatkę",
            noteId = newNote.Id
        });
    }

    [HttpGet("requests/{requestId}")]
    public async Task<IActionResult> GetRequestNotesAsync(int requestId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var requestEntity = await _db.Request
            .Include(r => r.Notes!)
            .ThenInclude(n => n.Author)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (requestEntity == null)
            return NotFound("Zgłoszenie nie istnieje");

        if (userRole != "admin" && requestEntity.UserId != userId && requestEntity.ManagerId != userId)
            return Forbid("Brak uprawnień do przeglądania notatek tego zgłoszenia");

        var notes = requestEntity.Notes?.OrderByDescending(n => n.CreatedAt).Select(note => new
        {
            id = note.Id,
            body = note.Body,
            createdAt = note.CreatedAt,
            isOwner = note.AuthorId == userId,
            author = new
            {
                id = note.AuthorId,
                name = $"{note.Author?.FirstName} {note.Author?.LastName}",
                email = note.Author?.Email
            }
        }).ToList();

        return Ok(notes);
    }

    [HttpGet("{noteId}")]
    public async Task<IActionResult> GetNoteAsync(int noteId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var note = await _db.Note
            .Include(n => n.Author)
            .Include(n => n.Request)
            .FirstOrDefaultAsync(n => n.Id == noteId);

        if (note == null)
            return NotFound("Notatka nie istnieje");

        if (userRole != "admin" && note.Request?.UserId != userId && note.Request?.ManagerId != userId)
            return Forbid("Brak uprawnień do przeglądania tej notatki");

        return Ok(new
        {
            id = note.Id,
            body = note.Body,
            createdAt = note.CreatedAt,
            isOwner = note.AuthorId == userId,
            author = new
            {
                id = note.AuthorId,
                name = $"{note.Author?.FirstName} {note.Author?.LastName}",
                email = note.Author?.Email
            }
        });
    }
    
    [HttpPut("{noteId}")]
    public async Task<IActionResult> UpdateNoteAsync(int noteId, [FromBody] UpdateNoteRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var note = await _db.Note
            .Include(n => n.Request)
            .FirstOrDefaultAsync(n => n.Id == noteId);

        if (note == null)
            return NotFound("Notatka nie istnieje");

        if (userRole != "admin" && note.Request?.UserId != userId && note.Request?.ManagerId != userId)
            return Forbid("Brak uprawnień do tego zgłoszenia");

        if (note.AuthorId != userId)
            return Forbid("Możesz edytować tylko swoje notatki");

        note.Body = request.Body;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Zaktualizowano notatkę"
        });
    }

    [HttpDelete("{noteId}")]
    public async Task<IActionResult> DeleteNoteAsync(int noteId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var note = await _db.Note
            .Include(n => n.Request)
            .FirstOrDefaultAsync(n => n.Id == noteId);

        if (note == null)
            return NotFound("Notatka nie istnieje");

        if (userRole != "admin" && note.Request?.UserId != userId && note.Request?.ManagerId != userId)
            return Forbid("Brak uprawnień do tego zgłoszenia");

        if (note.AuthorId != userId)
            return Forbid("Możesz usuwać tylko swoje notatki");

        _db.Note.Remove(note);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Usunięto notatkę"
        });
    }

    [HttpPatch("requests/{requestId}")]
    public async Task<IActionResult> UpdateRequestNoteAsync(int requestId, [FromBody] UpdateNoteRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var requestEntity = await _db.Request
            .Include(r => r.Notes)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (requestEntity == null)
            return NotFound("Zgłoszenie nie istnieje");

        if (userRole != "admin" && requestEntity.UserId != userId && requestEntity.ManagerId != userId)
            return Forbid("Brak uprawnień do edycji notatki zgłoszenia");

        var existingNote = requestEntity.Notes?
            .Where(n => n.AuthorId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefault();

        if (existingNote != null)
        {
            existingNote.Body = request.Body;
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
            await _db.Note.AddAsync(newNote);
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Zaktualizowano notatkę"
        });
    }
}
