using Microsoft.AspNetCore.Mvc;
using server.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using server.DTOs.Auth;
using System.Collections.Concurrent;
using server.DTOs.LoginRequest;
using server.Data;


[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private static readonly ConcurrentDictionary<string, List<DateTime>> _rateLimitStore = new();
    public AuthController(ApplicationDbContext db, IConfiguration config)
    {
        this._db = db;
        this._config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _db.User
                .Where(u => u.Email == request.Email)
                .Select(u => new User
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role,
                    PasswordHash = u.PasswordHash,
                }).FirstOrDefault();
                
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Nieprawidłowy email lub hasło" });
        }

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPatch("change-password")]
    [Authorize]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var clientId = GetClientId();
            if (IsRateLimited(clientId))
            {
                return StatusCode(429, new { success = false, error = "Przekroczono rate limie!" });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, error = "Nie ma usera!" });
            }

            var user = _db.User.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized(new { success = false, error = "Nie ma usera!" });
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { success = false, error = "Nieprawidłowe hasło" });
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return BadRequest(new { success = false, error = "Nowe hasła nie zgadzają się!" });
            }

            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
            {
                return BadRequest(new { success = false, error = "404 błąd serwera, spróbuj jeszcze raz" });
            }

            var passwordValidationResult = ValidatePasswordStrength(request.NewPassword);
            if (!passwordValidationResult.IsValid)
            {
                return BadRequest(new { success = false, error = passwordValidationResult.ErrorMessage });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _db.SaveChanges();
            return Ok(new { success = true, message = "Zmieniono hasło!" });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = "Interal server error!!" });
        }
    }
    private (bool IsValid, string ErrorMessage) ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return (false, "Hasło nie może być puste");
        }

        if (password.Length < 6)
        {
            return (false, "Hasło musi mieć przynajniej 6 znaków");
        }

        if (!password.Any(char.IsUpper))
        {
            return (false, "Hasło musi mieć przynjamniej jedną dużą litere!");
        }

        if (!password.Any(char.IsLower))
        {
            return (false, "Hasło musi mieć przynjmniej jedną małą litere!");
        }

        if (!password.Any(char.IsDigit))
        {
            return (false, "Hasło musi zawierać liczby");
        }

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return (false, "Hasło musi mieć znaki specjalne!");
        }

        return (true, string.Empty);
    }

    private string GetClientId()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        return $"{ipAddress}:{userId}";
    }

    private bool IsRateLimited(string clientId)
    {
        var now = DateTime.UtcNow;
        var timeWindow = TimeSpan.FromMinutes(15);
        var maxRequest = 5;
        var request = _rateLimitStore.GetOrAdd(clientId, new List<DateTime>());
        lock (request)
        {
            request.RemoveAll( r => r < now.Subtract(timeWindow));
            if (request.Count >= maxRequest)
            {
                return true;
            }

            request.Add(now);
            return false;
        }
    }
}
