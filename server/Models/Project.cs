using System;

namespace server.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsArchived { get; set; }
}