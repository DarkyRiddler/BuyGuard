namespace server.DTOs.Note;


/// <summary>
/// Żądanie aktualizacji notatki.
/// </summary>
public class UpdateNoteRequest
{
    /// <summary>Treść notatki</summary>
    public required string Body { get; set; }
}