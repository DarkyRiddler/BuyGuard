using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using server.Data;


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
    [HttpGet]
    // TODO Zmienić given_role_temporary na faktyczną zmienną po zrobieniu logowania
    public IActionResult GetUsers() // jak odczytać role jak jeszcze nie ma logowania idk 
    {
        var given_role = User.FindFirstValue(ClaimTypes.Role);
        if (given_role == null) return Unauthorized();

        if (given_role == "admin")
        {
            var users = _db.User
                .Where(u => u.Role == "manager")
                .Select(u => new
                {
                    Role = u.Role,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    ManagerLimitPln = u.ManagerLimitPln
                }).ToList();

            return Ok(new { user = users });
        }
        if (given_role == "manager")
        {
            var users = _db.User
                .Where(u => u.Role == "employee")
                .Select(u => new
                {
                    Role = u.Role,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                }).ToList();

            return Ok(new { user = users });
        }
        return Ok(new List<object>());
    }
    
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var user = _db.User
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                Role = u.Role,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                ManagerLimitPln = u.ManagerLimitPln
            })
            .FirstOrDefault();

        if (user == null) return NotFound();

        return Ok(new { user });
    }
    
    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

        if (currentUserRole == null)
            return NotFound();

        var user = _db.User.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
            return NotFound();

        if (currentUserRole == "admin" && user.Role != "manager")
            return Forbid();

        if (currentUserRole == "manager" && user.Role != "employee")
            return Forbid();

        
        _db.User.Remove(user);
        _db.SaveChanges();
        
        return Ok(new List<object>());
    }
    [Authorize]
    [HttpPost]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (currentUserRole == null)
            return Unauthorized();
        
        var existingUser = _db.User.FirstOrDefault(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return Conflict(new { message = "Mail zajęty!" });
        }

        string newUserRole;
        decimal? managerLimit = null;
        
        if (currentUserRole == "admin")
        {
            newUserRole = "manager";
            if (request.ManagerLimitPln == null)
            {
                return BadRequest(new { message = "Potrzebne informacje - limit menadżera!" });
            }
            managerLimit = request.ManagerLimitPln;
        }
        else if (currentUserRole == "manager")
        {
            newUserRole = "employee";
            if (request.ManagerLimitPln != null)
            {
                return BadRequest(new { message = "Nie można ustawić limitu dla nowego użytkownika!" });
            }
        }
        else
        {
            return Forbid(new { message = "Tylko admin i menedżer mogą tworzyć użytkowników" });
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = newUserRole,
            ManagerLimitPln = managerLimit
        };

        _db.User.Add(user);
        _db.SaveChanges();

        var response = new
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            ManagerLimitPln = user.ManagerLimitPln
        };

        return CreatedAtAction(nameof(GetUsers), response);
    }

    [Authorize]
    [HttpPatch("{id}")]
    public IActionResult UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (currentUserRole == null)
            return Unauthorized();

        var user = _db.User.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
            return NotFound(new { message = "Użytknownik not found!" });
        
        if (currentUserRole == "admin" && user.Role != "manager")
        {
            return Forbid(new { message = "Admin może edytować tylko menedżerów" });
        }
        else if (currentUserRole == "manager" && user.Role != "employee")
        {
            return Forbid(new { message = "Menedżer może edytować tylko pracowników" });
        }
        else if (currentUserRole != "admin" && currentUserRole != "manager")
        {
            return Forbid(new { message = "Tylko admin i menedżer mogą edytować użytkowników" });
        }
        
        if (!string.IsNullOrEmpty(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            user.LastName = request.LastName;
        }

        if (!string.IsNullOrEmpty(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }
        
        if (currentUserRole == "admin" && request.ManagerLimitPln.HasValue)
        {
            user.ManagerLimitPln = request.ManagerLimitPln;
        }

        _db.SaveChanges();

        var response = new
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            ManagerLimitPln = user.ManagerLimitPln
        };

        return Ok(response);
    }
    
}