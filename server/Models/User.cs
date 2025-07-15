using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Role { get; set; } = "employee";
    public decimal? ManagerLimitPln { get; set; } = 100000;

    public int? ManagerId { get; set; }
    public User? Manager { get; set; }
    public ICollection<User>? Subordinates { get; set; }

    public ICollection<Request>? Requests { get; set; }
    public ICollection<Request>? ManagedRequests { get; set; }
    public ICollection<Note>? Notes { get; set; }
}