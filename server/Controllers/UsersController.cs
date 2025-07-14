using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    // TODO Zmienić given_role_temporary na faktyczną zmienną po zrobieniu logowania
    public IActionResult GetUsers(string given_role_temporary) // jak odczytać role jak jeszcze nie ma logowania idk 
    {
        if (given_role_temporary == "admin")
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
        if (given_role_temporary == "manager")
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
    // TODO Zmienić id_of_current_user_temporary na token lub coś innego po zrobieniu logowania
    [HttpGet("me")]
    public IActionResult GetCurrentUser(int id_of_current_user_temporary)
    {
        var user = _db.User
        .Where(u => u.Id == id_of_current_user_temporary)
        .Select(u => new
        {
            Role = u.Role,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            ManagerLimitPln = u.ManagerLimitPln
        })
        .FirstOrDefault();
        if (user == null)
        {
            return Ok(new List<object>());
        }
        return Ok(new { user = user });
    }

    // delete usera po id
    /*
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
            
    }*/
}