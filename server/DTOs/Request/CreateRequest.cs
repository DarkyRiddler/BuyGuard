namespace server.DTOs.Request;


public class CreateRequest
{
    public string title { get; set; } = string.Empty;
    public string url { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal amount_pln { get; set; }
    public string reason { get; set; } = string.Empty;
}