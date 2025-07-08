using System;

namespace server.Models;

public class Note
{
    public int Id { get; set; }

    public int RequestId { get; set; }
    public Request Request { get; set; }

    public int AuthorId { get; set; }
    public User Author { get; set; }

    public string Body { get; set; }
    public DateTime CreatedAt { get; set; }
}