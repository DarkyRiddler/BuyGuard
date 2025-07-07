namespace server.Models;

public class IssueMeta
{
    public int Id { get; set; }

    public int IssueId { get; set; }
    public Issue Issue { get; set; }

    public string Url { get; set; }
    public string CookiesJson { get; set; }
    public string ConsoleLogsJson { get; set; }
    public string ScreenshotUrl { get; set; } // S3 or other storage
}