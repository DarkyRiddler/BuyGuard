using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } // "admin", "manager", "client"

    public int ProjectId { get; set; }
    public Project Project { get; set; }
}