namespace server.DTOs.User;


/// <summary>
/// Żądanie aktualizacji danych użytkownika.
/// </summary>
public class UpdateUserRequest
{
    /// <summary>Imię</summary>
    public string? FirstName { get; set; }

    /// <summary>Nazwisko</summary>
    public string? LastName { get; set; }

    /// <summary>Adres e-mail</summary>
    public string? Email { get; set; }

    /// <summary>Hasło</summary>
    public string? Password { get; set; }

    /// <summary>Limit menedżera w PLN</summary>
    public decimal? ManagerLimitPln { get; set; }
}