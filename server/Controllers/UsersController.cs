using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.DTOs.User;
using server.Data;
using Microsoft.EntityFrameworkCore;


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
    [Authorize]
    [HttpGet("{id}")]
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
    [Authorize]
    [HttpGet]
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

    [Authorize]
    [HttpGet("deleted")]
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
    [Authorize]
    [HttpGet("me")]
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

    [Authorize]
    [HttpDelete("{id}")]
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

    [Authorize]
    [HttpPost("{id}/restore")]
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
    [Authorize]
    [HttpPost]
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

    [Authorize]
    [HttpPatch("{id}")]
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
