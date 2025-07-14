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
}