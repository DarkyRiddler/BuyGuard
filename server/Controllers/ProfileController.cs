using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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



