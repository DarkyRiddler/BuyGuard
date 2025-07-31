namespace server.DTOs.LoginRequest;
/// <summary>
/// Żądanie logowania użytkownika.
/// </summary>
public class LoginRequest
{
    /// <summary>Email użytkownika</summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>Hasło użytkownika</summary>
    public string Password { get; set; } = string.Empty;
}