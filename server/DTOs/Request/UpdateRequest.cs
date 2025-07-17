namespace server.DTOs.Request;

public record class UpdateRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? AmountPln {get; set;}
    public string? Reason { get; set; }
}