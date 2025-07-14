<<<<<<< HEAD
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

=======
>>>>>>> b236d3fc7906b2324317a637b1a8d691951c7a5c
[Authorize]
[ApiController]
[Route("api/[controller]")]


public class ProfileController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // z tokena
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            Id = userId,
            Email = email,
            Role = role
        });
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin-data")]

    public IActionResult GetAdminData()
    {
        return Ok("To jest tylko dla admina");
    }


}



