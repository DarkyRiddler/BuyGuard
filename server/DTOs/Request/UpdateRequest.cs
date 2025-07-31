namespace server.DTOs.Request;

/// <summary>
/// Żądanie aktualizacji requestu.
/// </summary>
public class UpdateRequest
{
    /// <summary>Tytuł requestu</summary>
    public string? Title { get; set; }

    /// <summary>Opis requestu</summary>
    public string? Description { get; set; }

    /// <summary>Kwota w PLN</summary>
    public decimal? AmountPln { get; set; }

    /// <summary>Uzasadnienie zakupu</summary>
    public string? Reason { get; set; }
}