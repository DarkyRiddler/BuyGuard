namespace server.DTOs.User;

public record class UserDto
{
    public string Role { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public decimal? ManagerLimitPln { get; set; }
}
