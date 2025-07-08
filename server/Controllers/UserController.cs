using Microsoft.AspNetCore.Mvc;
using server.Data;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public UserController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok(new { user = _db.User.ToList() });
    }
}