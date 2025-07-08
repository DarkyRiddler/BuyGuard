using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;
public class Request
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int ManagerId { get; set; }
    public User Manager { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public decimal AmountPln { get; set; }
    public string Reason { get; set; }
    public string Status { get; set; }
    public double AiScore { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Attachment> Attachments { get; set; }
    public ICollection<Note> Notes { get; set; }
}