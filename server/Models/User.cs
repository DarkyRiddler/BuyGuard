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
    public required string Role { get; set; } = "employee"; // "admin", "manager", "employee", ewemtualnie "CEO"
    public decimal? ManagerLimitPln { get; set; }
    public ICollection<Request>? Requests { get; set; }
    public ICollection<Request>? ManagedRequests { get; set; }
    public ICollection<Note>? Notes { get; set; }
}