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
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;


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

    /// <summary>
    /// Loguje użytkownika do systemu.
    /// </summary>
    /// <remarks>
    /// Weryfikuje dane logowania użytkownika i tworzy token JWT.
    /// Token jest zwracany jako bezpieczne ciasteczko HTTP-only z czasem wygaśnięcia 1 godzina.
    /// Sprawdza czy konto nie zostało usunięte.
    /// </remarks>
    /// <param name="request">Dane logowania zawierające email i hasło</param>
    /// <returns>Komunikat potwierdzający logowanie</returns>
    /// <response code="200">Użytkownik został zalogowany pomyślnie</response>
    /// <response code="401">Nieprawidłowy email lub hasło, lub konto zostało usunięte</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(
        Summary = "Zaloguj użytkownika", 
        Description = "Weryfikuje dane logowania i tworzy token JWT przechowywany w bezpiecznym ciasteczku.")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.User
            .Where(u => u.Email == request.Email)
            .Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                PasswordHash = u.PasswordHash,
                IsDeleted = u.IsDeleted
            })
            .FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Nieprawidłowy email lub hasło" });
        }

        if (user.IsDeleted)
        {
            return Unauthorized(new { message = "Konto zostało usunięte" });
        }

        var token = GenerateJwtToken(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        };

        Response.Cookies.Append("jwt", token, cookieOptions);

        return Ok(new { message = "Zalogowano pomyślnie" });
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

    /// <summary>
    /// Zmienia hasło aktualnie zalogowanego użytkownika.
    /// </summary>
    /// <remarks>
    /// Pozwala na zmianę hasła po weryfikacji aktualnego hasła.
    /// Nowe hasło musi spełniać wymagania bezpieczeństwa:
    /// - minimum 6 znaków
    /// - co najmniej jedna wielka litera
    /// - co najmniej jedna mała litera
    /// - co najmniej jedna cyfra
    /// - co najmniej jeden znak specjalny
    /// 
    /// Ograniczenie rate limiting: maksymalnie 5 prób na 15 minut na IP/użytkownika.
    /// </remarks>
    /// <param name="request">Dane zawierające aktualne hasło, nowe hasło i potwierdzenie nowego hasła</param>
    /// <returns>Komunikat potwierdzający zmianę hasła</returns>
    /// <response code="200">Hasło zostało zmienione pomyślnie</response>
    /// <response code="400">Nieprawidłowe dane (złe aktualne hasło, niezgodne nowe hasła, słabe hasło)</response>
    /// <response code="401">Brak autoryzacji lub konto zostało usunięte</response>
    /// <response code="429">Przekroczono limit prób zmiany hasła (5 prób na 15 minut)</response>
    /// <response code="500">Błąd wewnętrzny serwera</response>
    [HttpPatch("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Zmień hasło użytkownika", 
        Description = "Zmienia hasło zalogowanego użytkownika z walidacją siły hasła i rate limiting.")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
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

            var user = await _db.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized(new { success = false, error = "Nie ma usera!" });
            }

            if (user.IsDeleted)
            {
                return Unauthorized(new { message = "Konto zostało usunięte" });
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
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Zmieniono hasło!" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, error = "Internal server error!!" });
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
    
    /// <summary>
    /// Wylogowuje użytkownika z systemu.
    /// </summary>
    /// <remarks>
    /// Usuwa token JWT poprzez wyczyszczenie ciasteczka z tokenem.
    /// Ustawia datę wygaśnięcia ciasteczka na przeszłość, co powoduje jego usunięcie przez przeglądarkę.
    /// Nie wymaga autoryzacji - można wylogować się nawet z nieważnym tokenem.
    /// </remarks>
    /// <returns>Komunikat potwierdzający wylogowanie</returns>
    /// <response code="200">Użytkownik został wylogowany pomyślnie</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Wyloguj użytkownika", 
        Description = "Usuwa token JWT poprzez wyczyszczenie ciasteczka. Nie wymaga autoryzacji.")]
    public IActionResult Logout()
    {
        Response.Cookies.Append("jwt", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTime.UtcNow.AddDays(-1),
            SameSite = SameSiteMode.None,
            Path = "/"
        });

        return Ok(new { success = true, message = "Wylogowano!" });
    }
}