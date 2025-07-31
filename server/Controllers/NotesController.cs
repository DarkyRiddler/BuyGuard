using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using server.DTOs.Note;
using server.Data;
using server.Models;
using server.Services;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly MailerService _mailerService;

    public NotesController(ApplicationDbContext db, MailerService mailerService)
    {
        this._db = db;
        this._mailerService = mailerService;
    }

    /// <summary>
    /// Tworzy notatkę do zgłoszenia.
    /// </summary>
    /// <param name="requestId">Identyfikator zgłoszenia</param>
    /// <param name="request">Dane notatki</param>
    /// <returns>Informacja o powodzeniu operacji</returns>
    /// <response code="200">Notatka utworzona</response>
    /// <response code="401">Nieautoryzowany użytkownik</response>
    /// <response code="403">Brak uprawnień</response>
    /// <response code="404">Zgłoszenie nie istnieje</response>
    [HttpPost("requests/{requestId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Tworzy notatkę", Description = "Dodaje notatkę do zgłoszenia, wysyła powiadomienia.")]
    public async Task<IActionResult> CreateNoteAsync(int requestId, [FromBody] CreateNoteRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var requestEntity = await _db.Request
            .Include(r => r.User)
            .Include(r => r.Manager)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (requestEntity == null)
            return NotFound("Zgłoszenie nie istnieje");

        if (userRole != "admin" && requestEntity.UserId != userId && requestEntity.ManagerId != userId)
            return Forbid("Brak uprawnień do dodawania notatek do tego zgłoszenia");
        var currentUser = await _db.User.FirstOrDefaultAsync(u => u.Id == userId);
        var newNote = new Note
        {
            RequestId = requestId,
            AuthorId = userId,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };

        await _db.Note.AddAsync(newNote);
        await _db.SaveChangesAsync();
        try
        {
            if (currentUser != null)
            {
                var noteAuthor = $"{currentUser.FirstName} {currentUser.LastName}";
                var ceoUsers = await _db.User.Where(u => u.Role == "admin").ToListAsync();
                var managerUsers = await _db.User.Where(u => u.Role == "manager").ToListAsync();
                
                var recipients = new List<User>();
                recipients.AddRange(ceoUsers.Where(u => u.Id != userId));
                recipients.AddRange(managerUsers.Where(u => u.Id != userId));
                
                if (requestEntity.Manager != null && requestEntity.ManagerId != userId)
                {
                    if (!recipients.Any(r => r.Id == requestEntity.ManagerId))
                    {
                        recipients.Add(requestEntity.Manager);
                    }
                }
                
                foreach (var recipient in recipients)
                {
                    var recipientName = $"{recipient.FirstName} {recipient.LastName}";
                    await _mailerService.SendNoteAddedNotificationAsync(
                        recipient.Email,
                        recipientName,
                        requestEntity.Title,
                        noteAuthor,
                        request.Body
                    );
                }
            }
        }
        catch (Exception ex)
        {
        }

        return Ok(new
        {
            success = true,
            message = "Dodano notatkę",
            noteId = newNote.Id
        });
    }
    /// <summary>
    /// Pobiera notatki zgłoszenia.
    /// </summary>
    /// <param name="requestId">Id zgłoszenia</param>
    /// <returns>Lista notatek</returns>
    /// <response code="200">Zwraca listę notatek</response>
    /// <response code="401">Nieautoryzowany użytkownik</response>
    /// <response code="403">Brak uprawnień</response>
    /// <response code="404">Zgłoszenie nie istnieje</response>
    [HttpGet("requests/{requestId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Pobiera notatki zgłoszenia", Description = "Zwraca listę notatek powiązanych ze zgłoszeniem.")]
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

    /// <summary>
    /// Pobiera pojedynczą notatkę.
    /// </summary>
    /// <param name="noteId">Id notatki</param>
    /// <returns>Dane notatki</returns>
    /// <response code="200">Zwraca notatkę</response>
    /// <response code="401">Nieautoryzowany użytkownik</response>
    /// <response code="403">Brak uprawnień</response>
    /// <response code="404">Notatka nie istnieje</response>
    [HttpGet("{noteId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Pobiera notatkę", Description = "Zwraca pojedynczą notatkę po Id.")]
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
    
    /// <summary>
    /// Aktualizuje notatkę.
    /// </summary>
    /// <param name="noteId">Id notatki</param>
    /// <param name="request">Dane aktualizacji</param>
    /// <returns>Informacja o powodzeniu</returns>
    /// <response code="200">Notatka zaktualizowana</response>
    /// <response code="401">Nieautoryzowany użytkownik</response>
    /// <response code="403">Brak uprawnień</response>
    /// <response code="404">Notatka nie istnieje</response>
    [HttpPut("{noteId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Aktualizuje notatkę", Description = "Modyfikuje istniejącą notatkę.")]
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

    /// <summary>
    /// Usuwa notatkę.
    /// </summary>
    /// <param name="noteId">Id notatki</param>
    /// <returns>Informacja o powodzeniu</returns>
    /// <response code="200">Notatka usunięta</response>
    /// <response code="401">Nieautoryzowany użytkownik</response>
    /// <response code="403">Brak uprawnień</response>
    /// <response code="404">Notatka nie istnieje</response>
    [HttpDelete("{noteId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Usuwa notatkę", Description = "Kasuje notatkę po Id.")]
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
    /// <summary>
    /// Tworzy lub aktualizuje notatkę użytkownika dla zgłoszenia.
    /// </summary>
    /// <param name="requestId">Identyfikator zgłoszenia</param>
    /// <param name="request">Dane notatki do zapisania</param>
    /// <returns>Informacja o powodzeniu operacji</returns>
    /// <response code="200">Notatka została utworzona lub zaktualizowana</response>
    /// <response code="401">Użytkownik niezalogowany</response>
    /// <response code="403">Brak uprawnień do modyfikacji notatek</response>
    /// <response code="404">Zgłoszenie nie istnieje</response>
    [HttpPatch("requests/{requestId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Aktualizuje lub dodaje notatkę użytkownika", Description = "Jeśli użytkownik ma już notatkę do zgłoszenia, zostanie zaktualizowana. W przeciwnym razie zostanie utworzona nowa.")]
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
