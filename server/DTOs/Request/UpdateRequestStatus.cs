using System.ComponentModel.DataAnnotations;
namespace server.DTOs.Request;


/// <summary>
/// Żądanie aktualizacji statusu requestu.
/// </summary>
public class UpdateRequestStatusDTO
{
    /// <summary>Status requestu (potwierdzono, odrzucono, zakupione)</summary>
    [RegularExpression("^(potwierdzono|odrzucono|zakupione)$", ErrorMessage = "Możliwe statusy: potwierdzono, odrzucono, zakupione")]
    public required string Status { get; set; }

    /// <summary>Uzasadnienie zmiany statusu</summary>
    public string? Reason { get; set; }
}