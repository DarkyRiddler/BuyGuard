using System.ComponentModel.DataAnnotations;
namespace server.DTOs.Request;

public record class UpdateRequestStatusDTO
{
    [RegularExpression("^(potwierdzono|odrzucono|zakupione)$",ErrorMessage = "Możliwe statusy: potwierdzono, odrzucono, zakupione")]
    public string Status { get; set; }
    public string? Reason { get; set; }
}