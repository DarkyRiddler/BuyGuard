namespace server.DTOs.Note;


/// <summary>
/// Żądanie utworzenia notatki.
/// </summary>
public class CreateNoteRequest
{
    /// <summary>Treść notatki</summary>
    public required string Body { get; set; }
}
