using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.DTOs.User;
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
    public IActionResult GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var given_role = User.FindFirstValue(ClaimTypes.Role);
        if (given_role == null) return Unauthorized();
        IQueryable<object> usersQuery;
        if (given_role == "admin")
        {
            usersQuery = _db.User
                .Where(u => u.Role == "manager")
                .Select(u => new
                {
                    Id = u.Id,
                    Role = u.Role,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    ManagerLimitPln = u.ManagerLimitPln
                });
        }
        else if (given_role == "manager")
        {
            usersQuery = _db.User
                .Where(u => u.Role == "employee")
                .Select(u => new
                {
                    Id = u.Id,
                    Role = u.Role,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
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

        var totalUsers = usersQuery.Count();
        var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
        var users = usersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Ok(new
        {
            user = users,
            totalPages = totalPages,
            currentPage = page,
            totalUsers = totalUsers
        });
    }
    /*
     stare bez paginacji - mozna usunac jbc
     public IActionResult GetUsers() 
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
    }*/
    
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
            return Conflict("Mail zajęty!");
        }

        string newUserRole;
        decimal? managerLimit = null;
        
        if (currentUserRole == "admin")
        {
            newUserRole = "manager";
            if (request.ManagerLimitPln == null)
            {
                return BadRequest("Potrzebne informacje - limit menadżera!");
            }
            managerLimit = request.ManagerLimitPln;
        }
        else if (currentUserRole == "manager")
        {
            newUserRole = "employee";
            if (request.ManagerLimitPln != null)
            {
                return BadRequest("Nie można ustawić limitu dla nowego użytkownika!");
            }
        }
        else
        {
            return Forbid("Tylko admin i menedżer mogą tworzyć użytkowników");
        }

        var newUser = new server.Models.User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = newUserRole,
            ManagerLimitPln = managerLimit
        };

        _db.User.Add(newUser);
        _db.SaveChanges();

        var response = new
        {
            Id = newUser.Id,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Email = newUser.Email,
            Role = newUser.Role,
            ManagerLimitPln = newUser.ManagerLimitPln
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

        var userToUpdate = _db.User.FirstOrDefault(u => u.Id == id);
        
        if (userToUpdate == null)
            return NotFound("Użytknownik not found!");
        
        if (currentUserRole == "admin" && userToUpdate.Role != "manager")
        {
            return Forbid("Admin może edytować tylko menedżerów");
        }
        else if (currentUserRole == "manager" && userToUpdate.Role != "employee")
        {
            return Forbid("Menedżer może edytować tylko pracowników");
        }
        else if (currentUserRole != "admin" && currentUserRole != "manager")
        {
            return Forbid("Tylko admin i menedżer mogą edytować użytkowników");
        }
        
        if (!string.IsNullOrEmpty(request.FirstName))
        {
            userToUpdate.FirstName = request.FirstName;
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            userToUpdate.LastName = request.LastName;
        }

        if (!string.IsNullOrEmpty(request.Password))
        {
            userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }
        
        if (currentUserRole == "admin" && request.ManagerLimitPln.HasValue)
        {
            userToUpdate.ManagerLimitPln = request.ManagerLimitPln;
        }

        _db.SaveChanges();

        var response = new
        {
            Id = userToUpdate.Id,
            FirstName = userToUpdate.FirstName,
            LastName = userToUpdate.LastName,
            Email = userToUpdate.Email,
            Role = userToUpdate.Role,
            ManagerLimitPln = userToUpdate.ManagerLimitPln
        };

        return Ok(response);
    }
   
}