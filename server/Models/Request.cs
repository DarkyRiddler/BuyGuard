using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;
public class Request
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public required User User { get; set; }

    public int ManagerId { get; set; }
    public User? Manager { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }
    public decimal AmountPln { get; set; }
    public required string Reason { get; set; }
    public required string Status { get; set; }
    public double? AiScore { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Attachment>? Attachments { get; set; }
    public ICollection<Note>? Notes { get; set; }
}