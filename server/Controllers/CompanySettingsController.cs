using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.DTOs.CompanySettings;
using server.Data;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<IActionResult> GetCompanySettings()
        {
            var companySettings = await _db.CompanySettings.FirstOrDefaultAsync();
            
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

        [HttpPut]
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