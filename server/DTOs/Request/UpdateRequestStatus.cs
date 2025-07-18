using System.ComponentModel.DataAnnotations;
namespace server.DTOs.Request;

public class UpdateRequestStatusDTO
{
    [RegularExpression("^(potwierdzono|odrzucono|zakupione)$",ErrorMessage = "Możliwe statusy: potwierdzono, odrzucono, zakupione")]
    public required string Status { get; set; }
    public string? Reason { get; set; }
}