using System.ComponentModel.DataAnnotations;

namespace server.Models
{
    public class CompanySettings
    {
        public int Id { get; set; }
        
        [Required]
        public string CompanyName { get; set; } = string.Empty;
        
        public string? CompanyDescription { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}