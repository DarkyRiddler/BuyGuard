namespace server.DTOs.CompanySettings
{
    public class CompanySettingsResponse
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}