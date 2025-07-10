using Microsoft.AspNetCore.Mvc;
using server.Data;
using server.DTOs.User;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public UserController(ApplicationDbContext db)
    {
        this._db = db;
    }

    [HttpGet]
    public IActionResult GetUsers(string given_role_temporary) // jak odczytać role jak jeszcze nie ma logowania idk 
    {
        if (given_role_temporary == "admin")
        {
            var users = _db.User
                .Where(u => u.Role == "manager")
                .Select(u => new UserDto
                {
                    Role = u.Role,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    ManagerLimitPln = u.ManagerLimitPln
                }).ToList();

            return Ok(new{user = users});
        }
        if (given_role_temporary == "manager")
        {
            var users = _db.User
                .Where(u => u.Role == "client")
                .Select(u => new UserDto
                {
                    Role = u.Role,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                }).ToList();

            return Ok(new{user = users});
        }
        return Ok(new List<object>());
    }
    
}