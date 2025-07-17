namespace server.DTOs.User;

public record class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Password { get; set; }
    public decimal? ManagerLimitPln { get; set; }
}