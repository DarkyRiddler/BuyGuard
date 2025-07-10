namespace server.Models;

public class Attachment
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public Request Request { get; set; }
    public string FileUrl { get; set; }
    public string MimeType { get; set; }
}