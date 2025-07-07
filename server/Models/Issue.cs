using System;

namespace server.Models;

public class Issue
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public IssueMeta IssueMeta { get; set; }
}