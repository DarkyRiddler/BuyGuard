using System;

namespace server.Models;

public class Note
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public required Request Request { get; set; }
    public int AuthorId { get; set; }
    public required User Author { get; set; }
    public required string Body { get; set; }
    public DateTime CreatedAt { get; set; }
}