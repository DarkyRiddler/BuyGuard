using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; } // "admin", "manager", "client", ewemtualnie "CEO"
    public decimal ManagerLimitPln { get; set; }
    public ICollection<Request> Requests { get; set; }
    public ICollection<Request> ManagedRequests { get; set; }
    public ICollection<Note> Notes { get; set; }
}