using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.DTOs.User;
using server.Data;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;


namespace server.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public UsersController(ApplicationDbContext db)
    {
        this._db = db;
    }

    /// <summary>
    /// Pobiera dane konkretnego użytkownika.
    /// </summary>
    /// <remarks>
    /// Zwraca informacje o użytkowniku na podstawie ID.
    /// Dostęp ograniczony według ról:
    /// - admin: może przeglądać tylko managerów
    /// - manager: może przeglądać tylko pracowników
    /// </remarks>
    /// <param name="id">Identyfikator użytkownika</param>
    /// <returns>Dane użytkownika</returns>
    /// <response code="200">Użytkownik został znaleziony</response>
    /// <response code="401">Brak autoryzacji</response>
    /// <response code="403">Brak dostępu do tego użytkownika</response>
    /// <response code="404">Użytkownik nie został znaleziony lub jest usunięty</response>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Pobierz dane użytkownika",
        Description = "Pobiera informacje o konkretnym użytkowniku na podstawie roli zalogowanego użytkownika.")]
    public async Task<IActionResult> GetUser(int id)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (currentUserRole == null)
            return Unauthorized();

        var user = await _db.User.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null || user.IsDeleted)
            return NotFound();
        if (currentUserRole == "admin" && user.Role != "manager")
            return Forbid();
        if (currentUserRole == "manager" && user.Role != "employee")
            return Forbid();
        return Ok(new
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            ManagerLimitPln = user.ManagerLimitPln
        });
    }

    /// <summary>
    /// Pobiera listę użytkowników z paginacją.
    /// </summary>
    /// <remarks>
    /// Zwraca paginowaną listę użytkowników w zależności od roli:
    /// - admin: widzi listę managerów
    /// - manager: widzi listę pracowników
    /// - employee: brak dostępu
    /// </remarks>
    /// <param name="page">Numer strony (domyślnie 1)</param>
    /// <param name="pageSize">Liczba elementów na stronie (domyślnie 10)</param>
    /// <returns>Paginowana lista użytkowników</returns>
    /// <response code="200">Lista użytkowników została pobrana</response>
    /// <response code="401">Brak autoryzacji</response>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(
        Summary = "Pobierz listę użytkowników",
        Description = "Pobiera paginowaną listę użytkowników w zależności od roli zalogowanego użytkownika.")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var given_role = User.FindFirstValue(ClaimTypes.Role);
        if (given_role == null) return Unauthorized();

        IQueryable<object> usersQuery;

        if (given_role == "admin")
        {
            usersQuery = _db.User
                .Where(u => u.Role == "manager" && !u.IsDeleted)
                .Select(u => new
                {
                    u.Id,
                    u.Role,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.ManagerLimitPln
                });
        }
        else if (given_role == "manager")
        {
            usersQuery = _db.User
                .Where(u => u.Role == "employee" && !u.IsDeleted)
                .Select(u => new
                {
                    u.Id,
                    u.Role,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    ManagerLimitPln = (decimal?)null
                });
        }
        else
        {
            return Ok(new
            {
                user = new List<object>(),
                totalPages = 0,
                currentPage = page,
                totalUsers = 0
            });
        }

        var totalUsers = await usersQuery.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
        var users = await usersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            user = users,
            totalPages,
            currentPage = page,
            totalUsers
        });
    }


    /// <summary>
    /// Pobiera listę usuniętych użytkowników.
    /// </summary>
    /// <remarks>
    /// Zwraca paginowaną listę usuniętych użytkowników.
    /// Dostępne tylko dla administratorów.
    /// Nie zawiera administratorów w wynikach.
    /// </remarks>
    /// <param name="page">Numer strony (domyślnie 1)</param>
    /// <param name="pageSize">Liczba elementów na stronie (domyślnie 10)</param>
    /// <returns>Paginowana lista usuniętych użytkowników</returns>
    /// <response code="200">Lista usuniętych użytkowników została pobrana</response>
    /// <response code="403">Tylko administrator może przeglądać usuniętych użytkowników</response>
    [Authorize]
    [HttpGet("deleted")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Pobierz listę usuniętych użytkowników",
        Description = "Pobiera paginowaną listę usuniętych użytkowników. Dostępne tylko dla administratorów.")]
    public async Task<IActionResult> GetDeletedUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (currentUserRole != "admin")
            return Forbid("Tylko CEO może przeglądać deleted userów1");
        var usersQuery = _db.User
            .Where(u => u.IsDeleted && u.Role != "admin")
            .Select(u => new
            {
                u.Id,
                u.Role,
                u.FirstName,
                u.LastName,
                u.Email,
                u.ManagerLimitPln
            });
        var totalUsers = await usersQuery.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
        var users = await usersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(new
        {
            user = users,
            totalPages,
            currentPage = page,
            totalUsers
        });
    }


    /// <summary>
    /// Pobiera dane aktualnie zalogowanego użytkownika.
    /// </summary>
    /// <remarks>
    /// Zwraca informacje o aktualnie zalogowanym użytkowniku na podstawie tokenu JWT.
    /// </remarks>
    /// <returns>Dane aktualnie zalogowanego użytkownika</returns>
    /// <response code="200">Dane użytkownika zostały pobrane</response>
    /// <response code="401">Brak autoryzacji lub nieprawidłowy token</response>
    /// <response code="404">Użytkownik nie został znaleziony lub jest usunięty</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Pobierz dane aktualnego użytkownika",
        Description = "Pobiera informacje o aktualnie zalogowanym użytkowniku.")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var user = await _db.User
            .Where(u => u.Id == userId && !u.IsDeleted)
            .Select(u => new
            {
                u.Id,
                u.Role,
                u.FirstName,
                u.LastName,
                u.Email,
                u.ManagerLimitPln
            })
            .FirstOrDefaultAsync();

        if (user == null) return NotFound();

        return Ok(new { user });
    }


    /// <summary>
    /// Usuwa użytkownika (oznacza jako usunięty).
    /// </summary>
    /// <remarks>
    /// Oznacza użytkownika jako usunięty (soft delete).
    /// Dostęp ograniczony według ról:
    /// - admin: może usuwać tylko managerów
    /// - manager: może usuwać tylko pracowników
    /// </remarks>
    /// <param name="id">Identyfikator użytkownika do usunięcia</param>
    /// <returns>Potwierdzenie usunięcia</returns>
    /// <response code="200">Użytkownik został usunięty</response>
    /// <response code="403">Brak uprawnień do usunięcia tego użytkownika</response>
    /// <response code="404">Użytkownik nie został znaleziony lub jest już usunięty</response>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Usuń użytkownika",
        Description = "Oznacza użytkownika jako usunięty zgodnie z uprawnieniami roli.")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (currentUserRole == null) return NotFound();

        var user = await _db.User.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null || user.IsDeleted) return NotFound();

        if (currentUserRole == "admin" && user.Role != "manager") return Forbid();
        if (currentUserRole == "manager" && user.Role != "employee") return Forbid();

        user.IsDeleted = true;
        _db.User.Update(user);
        await _db.SaveChangesAsync();

        return Ok(new List<object>());
    }


    /// <summary>
    /// Przywraca usuniętego użytkownika.
    /// </summary>
    /// <remarks>
    /// Przywraca użytkownika oznaczonego jako usunięty.
    /// Dostępne tylko dla administratorów.
    /// Nie można przywrócić administratorów.
    /// </remarks>
    /// <param name="id">Identyfikator użytkownika do przywrócenia</param>
    /// <returns>Potwierdzenie przywrócenia</returns>
    /// <response code="200">Użytkownik został przywrócony</response>
    /// <response code="403">Tylko administrator może przywracać użytkowników lub próba przywrócenia administratora</response>
    /// <response code="404">Użytkownik nie został znaleziony lub nie jest usunięty</response>
    [Authorize]
    [HttpPost("{id}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Przywróć usuniętego użytkownika",
        Description = "Przywraca użytkownika oznaczonego jako usunięty. Dostępne tylko dla administratorów.")]
    public async Task<IActionResult> RestoreUser(int id)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (currentUserRole != "admin") return Forbid("Tylko CEO może przywracać M/U");
        var user = await _db.User.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null || !user.IsDeleted) return NotFound();
        if (user.Role == "admin") return Forbid("Admina nie przywracamy!");
        user.IsDeleted = false;
        _db.User.Update(user);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Przywrócono konto użytkownika" });
    }
  
    /// <summary>
    /// Tworzy nowego użytkownika.
    /// </summary>
    /// <remarks>
    /// Tworzy nowego użytkownika w systemie.
    /// Rola nowego użytkownika zależy od roli twórcy:
    /// - admin: tworzy managerów (wymagany limit managera)
    /// - manager: tworzy pracowników (bez limitu)
    /// Email musi być unikalny w systemie.
    /// </remarks>
    /// <param name="request">Dane nowego użytkownika</param>
    /// <returns>Dane utworzonego użytkownika</returns>
    /// <response code="201">Użytkownik został utworzony</response>
    /// <response code="400">Błędne dane lub brakujący limit managera</response>
    /// <response code="401">Brak autoryzacji</response>
    /// <response code="403">Brak uprawnień do tworzenia użytkowników</response>
    /// <response code="409">Email jest już zajęty</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [SwaggerOperation(
        Summary = "Utwórz nowego użytkownika",
        Description = "Tworzy nowego użytkownika z rolą zależną od uprawnień twórcy.")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (currentUserRole == null)
            return Unauthorized();

        var existingUser = await _db.User.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
            return Conflict("Mail zajęty!");

        string newUserRole;
        decimal? managerLimit = null;

        if (currentUserRole == "admin")
        {
            newUserRole = "manager";
            if (request.ManagerLimitPln == null)
                return BadRequest("Potrzebne informacje - limit menadżera!");
            managerLimit = request.ManagerLimitPln;
        }
        else if (currentUserRole == "manager")
        {
            newUserRole = "employee";
            if (request.ManagerLimitPln != null)
                return BadRequest("Nie można ustawić limitu dla nowego użytkownika!");
        }
        else return Forbid("Tylko admin i menedżer mogą tworzyć użytkowników");

        var newUser = new server.Models.User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = newUserRole,
            ManagerLimitPln = managerLimit
        };

        await _db.User.AddAsync(newUser);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsers), new
        {
            newUser.Id,
            newUser.FirstName,
            newUser.LastName,
            newUser.Email,
            newUser.Role,
            newUser.ManagerLimitPln
        });
    }


    /// Aktualizuje dane użytkownika.
    /// </summary>
    /// <remarks>
    /// Aktualizuje dane istniejącego użytkownika.
    /// Dostęp ograniczony według ról:
    /// - admin: może edytować tylko managerów (w tym limit managera)
    /// - manager: może edytować tylko pracowników
    /// Email musi być unikalny w systemie.
    /// </remarks>
    /// <param name="id">Identyfikator użytkownika do aktualizacji</param>
    /// <param name="request">Nowe dane użytkownika</param>
    /// <returns>Zaktualizowane dane użytkownika</returns>
    /// <response code="200">Użytkownik został zaktualizowany</response>
    /// <response code="401">Brak autoryzacji</response>
    /// <response code="403">Brak uprawnień do edycji tego użytkownika</response>
    /// <response code="404">Użytkownik nie został znaleziony lub jest usunięty</response>
    /// <response code="409">Email jest już zajęty</response>
    [Authorize]
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [SwaggerOperation(
        Summary = "Aktualizuj dane użytkownika",
        Description = "Aktualizuje dane użytkownika zgodnie z uprawnieniami roli.")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (currentUserRole == null)
            return Unauthorized();

        var userToUpdate = await _db.User.FirstOrDefaultAsync(u => u.Id == id);
        if (userToUpdate == null || userToUpdate.IsDeleted)
            return NotFound("Użytknownik not found!");

        if (currentUserRole == "admin" && userToUpdate.Role != "manager")
            return Forbid("Admin może edytować tylko menedżerów");
        if (currentUserRole == "manager" && userToUpdate.Role != "employee")
            return Forbid("Menedżer może edytować tylko pracowników");
        if (currentUserRole != "admin" && currentUserRole != "manager")
            return Forbid("Tylko admin i menedżer mogą edytować użytkowników");

        if (!string.IsNullOrEmpty(request.Email) && request.Email != userToUpdate.Email)
        {
            var existingUser = await _db.User.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
                return Conflict("Email jest już zajęty");
            userToUpdate.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.FirstName))
            userToUpdate.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName))
            userToUpdate.LastName = request.LastName;
        if (!string.IsNullOrEmpty(request.Password))
            userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        if (currentUserRole == "admin" && request.ManagerLimitPln.HasValue)
            userToUpdate.ManagerLimitPln = request.ManagerLimitPln;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            userToUpdate.Id,
            userToUpdate.FirstName,
            userToUpdate.LastName,
            userToUpdate.Email,
            userToUpdate.Role,
            userToUpdate.ManagerLimitPln
        });
    }
}
