namespace server.DTOs.Request;


/// <summary>
/// Żądanie utworzenia requestu.
/// </summary>
public class CreateRequest
{
    /// <summary>Tytuł requestu</summary>
    public string title { get; set; } = string.Empty;

    /// <summary>Adres URL do produktu</summary>
    public string url { get; set; } = string.Empty;

    /// <summary>Opis requestu</summary>
    public string description { get; set; } = string.Empty;

    /// <summary>Kwota w PLN</summary>
    public decimal amount_pln { get; set; }

    /// <summary>Uzasadnienie zakupu</summary>
    public string reason { get; set; } = string.Empty;
}