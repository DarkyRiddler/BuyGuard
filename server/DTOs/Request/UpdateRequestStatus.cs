namespace server.DTOs.Request;

public class UpdateRequestStatus
{
    [RegularExpression("^(potwierdzono|odrzucono|zakupione)$",ErrorMessage = "Możliwe statusy: potwierdzono, odrzucono, zakupione")]
    public string Status { get; set; }
    public string? Reason { get; set; }
}