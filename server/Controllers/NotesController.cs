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
    public IActionResult CreateNote(int requestId, [FromBody] CreateNoteRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var requestEntity = _db.Request.FirstOrDefault(r => r.Id == requestId);
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

        _db.Note.Add(newNote);
        _db.SaveChanges();

        return Ok(new
        {
            success = true,
            message = "Dodano notsa",
            noteId = newNote.Id
        });
    }

    [HttpGet("requests/{requestId}")]
    public IActionResult GetRequestNotes(int requestId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var requestEntity = _db.Request
            .Include(r => r.Notes!)
            .ThenInclude(n => n.Author)
            .FirstOrDefault(r => r.Id == requestId);

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
    public IActionResult GetNote(int noteId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var note = _db.Note
            .Include(n => n.Author)
            .Include(n => n.Request)
            .FirstOrDefault(n => n.Id == noteId);

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
    public IActionResult UpdateNote(int noteId, [FromBody] UpdateNoteRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var note = _db.Note
            .Include(n => n.Request)
            .FirstOrDefault(n => n.Id == noteId);

        if (note == null)
            return NotFound("Notatka nie istnieje");

        if (userRole != "admin" && note.Request?.UserId != userId && note.Request?.ManagerId != userId)
            return Forbid("Brak uprawnień do tego zgłoszenia");

        if (note.AuthorId != userId)
            return Forbid("Możesz edytować tylko swoje notatki");

        note.Body = request.Body;
        _db.SaveChanges();

        return Ok(new
        {
            success = true,
            message = "Zaktualizowano notsa"
        });
    }

    [HttpDelete("{noteId}")]
    public IActionResult DeleteNote(int noteId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var note = _db.Note
            .Include(n => n.Request)
            .FirstOrDefault(n => n.Id == noteId);

        if (note == null)
            return NotFound("Notatka nie istnieje");

        if (userRole != "admin" && note.Request?.UserId != userId && note.Request?.ManagerId != userId)
            return Forbid("Brak uprawnień do tego zgłoszenia");
        
        if (note.AuthorId != userId)
            return Forbid("Możesz usuwać tylko swoje notatki");

        _db.Note.Remove(note);
        _db.SaveChanges();

        return Ok(new
        {
            success = true,
            message = "Usunieto notsa"
        });
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
            _db.Note.Add(newNote);
        }

        _db.SaveChanges();
        return Ok(new
        {
            success = true,
            message = "Zaktualizowano notsa"
        });
    }
}