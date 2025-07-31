using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.DTOs.CompanySettings;
using server.Data;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CompanySettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CompanySettingsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Pobiera ustawienia firmy.
        /// </summary>
        /// <remarks>
        /// Zwraca aktualne ustawienia konfiguracyjne firmy.
        /// Dostępne dla admina 
        /// </remarks>
        /// <returns>Obiekt z danymi firmy</returns>
        /// <response code="200">Zwraca dane firmy</response>
        /// <response code="404">Nie znaleziono ustawień firmy</response>
        [HttpGet]
        [ProducesResponseType(typeof(CompanySettingsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Pobiera ustawienia firmy", Description = "Zwraca dane konfiguracyjne firmy.")]
        public async Task<IActionResult> GetCompanySettings()
        {
            var companySettings = await _db.CompanySettings.FirstOrDefaultAsync();
            // NIE MA ZROBIONEGO SPRAWDZANIA ROLI !!!
            if (companySettings == null)
            {
                return NotFound(new { message = "Ustawienia firmy nie zostały znalezione" });
            }

            var response = new CompanySettingsResponse
            {
                Id = companySettings.Id,
                CompanyName = companySettings.CompanyName,
                CompanyDescription = companySettings.CompanyDescription,
                CreatedAt = companySettings.CreatedAt,
                UpdatedAt = companySettings.UpdatedAt
            };

            return Ok(response);
        }

        /// <summary>
        /// Aktualizuje ustawienia firmy.
        /// </summary>
        /// <remarks>
        /// Aktualizuje nazwę i opis firmy.
        /// Dostępne tylko dla administratorów.
        /// Wszystkie pola w żądaniu są wymagane.
        /// </remarks>
        /// <param name="request">Dane do aktualizacji zawierające nazwę i opis firmy</param>
        /// <returns>Zaktualizowane ustawienia firmy z komunikatem potwierdzającym</returns>
        /// <response code="200">Ustawienia firmy zostały zaktualizowane</response>
        /// <response code="400">Błędne dane wejściowe lub nieprawidłowa walidacja</response>
        /// <response code="401">Brak autoryzacji</response>
        /// <response code="403">Tylko administrator może modyfikować ustawienia firmy</response>
        /// <response code="404">Ustawienia firmy nie zostały znalezione</response>
        /// <response code="500">Błąd serwera podczas aktualizacji</response>
        [HttpPut]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Aktualizuj ustawienia firmy", 
            Description = "Aktualizuje nazwę i opis firmy. Dostęp ograniczony tylko do administratorów.")]
        public async Task<IActionResult> UpdateCompanySettings([FromBody] UpdateCompanySettingsRequest request)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            if (currentUserRole != "admin")
            {
                return Forbid("Tylko administrator może modyfikować ustawienia firmy");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var companySettings = await _db.CompanySettings.FirstOrDefaultAsync();
            
            if (companySettings == null)
            {
                return NotFound(new { message = "Ustawienia firmy nie zostały znalezione" });
            }
            
            companySettings.CompanyName = request.CompanyName;
            companySettings.CompanyDescription = request.CompanyDescription;
            companySettings.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();

                var response = new CompanySettingsResponse
                {
                    Id = companySettings.Id,
                    CompanyName = companySettings.CompanyName,
                    CompanyDescription = companySettings.CompanyDescription,
                    CreatedAt = companySettings.CreatedAt,
                    UpdatedAt = companySettings.UpdatedAt
                };

                return Ok(new 
                { 
                    message = "Ustawienia firmy zostały pomyślnie zaktualizowane",
                    data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas aktualizacji ustawień firmy", error = ex.Message });
            }
        }
    }
}