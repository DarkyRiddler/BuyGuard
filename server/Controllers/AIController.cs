using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.Data;
using server.Services;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAIService _aiService;

    public AIController(ApplicationDbContext db, IAIService aiService)
    {
        _db = db;
        _aiService = aiService;
    }
    /// <summary>
    /// Generuje brakujące wartości AI Score dla zgłoszeń bez wyniku.
    /// </summary>
    /// <remarks>
    /// Operacja dostępna tylko dla użytkowników z rolą admin.
    /// </remarks>
    /// <response code="200">Sukces — wygenerowano brakujące AI scores</response>
    /// <response code="403">Brak uprawnień — tylko admin może wykonać tę operację</response>
    /// <response code="400">Błąd generowania wyników</response>
    [HttpPost("generate-missing-scores")]
    public async Task<IActionResult> GenerateMissingAIScores()
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userRole != "admin")
            return Forbid("Tylko admin może generować brakujące AI scores");

        try
        {
            var successCount = await _aiService.GenerateMissingAIScores(_db);
            return Ok(new
            {
                success = true,
                message = $"Pomyślnie wygenerowano AI scores dla {successCount} requestów",
                processedCount = successCount
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Błąd podczas generowania AI scores: {ex.Message}");
        }
    }
}