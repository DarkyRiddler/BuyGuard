namespace server.DTOs.User;

/// <summary>
/// Żądanie utworzenia użytkownika.
/// </summary>
public class CreateUserRequest
{
    /// <summary>Imię</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Nazwisko</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Adres e-mail</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Hasło</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Limit menedżera w PLN</summary>
    public decimal? ManagerLimitPln { get; set; }
}
