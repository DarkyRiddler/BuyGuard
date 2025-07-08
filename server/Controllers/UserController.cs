using Microsoft.AspNetCore.Mvc;
using server.DTOs.User;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryParameters parameters)
    {
        var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        if (userRole == null)
            return Forbid();

        var users = await _userService.GetUsersAsync(userRole, parameters);
        return Ok(users);
    }
}