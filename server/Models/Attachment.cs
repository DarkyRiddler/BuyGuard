namespace server.Models;

public class Attachment
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public Request? Request { get; set; }
    public required string FileUrl { get; set; }
    public required string MimeType { get; set; }
}