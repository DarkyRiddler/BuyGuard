using System.ComponentModel.DataAnnotations;

namespace server.DTOs.CompanySettings
{
    /// <summary>
    /// Żądanie aktualizacji ustawień firmy.
    /// </summary>
    public class UpdateCompanySettingsRequest
    {
        /// <summary>Nazwa firmy</summary>
        [Required(ErrorMessage = "Nazwa firmy jest wymagana")]
        [StringLength(200, ErrorMessage = "Nazwa firmy nie może przekraczać 200 znaków")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>Opis firmy</summary>
        [StringLength(1000, ErrorMessage = "Opis firmy nie może przekraczać 1000 znaków")]
        public string? CompanyDescription { get; set; }
    }
}