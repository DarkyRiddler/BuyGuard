using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.Data;
using System.Text;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

// TODO osobno zrobić monthly i osobno zrobić ogólny 

[ApiController]
[Route("api/[controller]")]
[Authorize] // tylko admin moze eksportować dane
public class PerlaController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    
    public PerlaController(ApplicationDbContext db)
    {
        this._db = db;
        }
    [HttpGet("export")]
    public async Task<IActionResult> ExportRequests()
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();

        var requestsQuery = await _db.Request
            .Where(r => r.Status == "zakupione")
            .Select(r => new
            {
                id = r.Id,
                title = r.Title,
                description = r.Description,
                amountPln = r.AmountPln,
                reason = r.Reason,
                aiScore = r.AiScore,
                createdAt = r.CreatedAt,
                updatedAt = r.UpdatedAt,
                userId = r.UserId,
                userFirstName = r.User.FirstName,
                userLastName = r.User.LastName,
                userEmail = r.User.Email,
                managerId = r.ManagerId,
                managerFirstName = r.Manager != null ? r.Manager.FirstName : null,
                managerLastName = r.Manager != null ? r.Manager.LastName : null,
                managerEmail = r.Manager != null ? r.Manager.Email : null,
            })
            .ToListAsync();

        var requests = requestsQuery.Select(r => new
        {
            r.id,
            r.title,
            r.description,
            r.amountPln,
            r.reason,
            r.aiScore,
            r.createdAt,
            r.updatedAt,
            r.userId,
            userName = $"{r.userFirstName} {r.userLastName}",
            r.userEmail,
            r.managerId,
            managerName = r.managerFirstName != null ? $"{r.managerFirstName} {r.managerLastName}" : null,
            r.managerEmail
        }).ToList();

        var sbData = new StringBuilder();
        var sep = ";";
        sbData.AppendLine(string.Join(sep, "Id", "Title", "Description", "AmountPln", "Reason",
                                    "AiScore", "CreatedAt", "UpdatedAt", "UserId", "UserName",
                                    "UserEmail", "ManagerId", "ManagerName", "ManagerEmail"));
        foreach (var r in requests)
        {
            sbData.AppendLine(string.Join(sep, r.id, r.title, r.description, r.amountPln, r.reason,
                                            r.aiScore, r.createdAt.ToString("dd-MM-yyyy HH:mm:ss"),
                                            r.updatedAt?.ToString("dd-MM-yyyy HH:mm:ss") ?? "",
                                            r.userId, r.userName, r.userEmail,
                                            r.managerId, r.managerName ?? "", r.managerEmail ?? ""));
        }
        var sbStats = new StringBuilder();
        sbStats.AppendLine("Statystyka,Wartość");
        sbStats.AppendLine($"Liczba zgłoszeń,{requests.Count}");
        sbStats.AppendLine($"Suma kwot,{requests.Sum(r => r.amountPln).ToString(CultureInfo.InvariantCulture)}");
        sbStats.AppendLine($"Średnia kwota,{(requests.Count > 0 ? requests.Average(r => r.amountPln).ToString(CultureInfo.InvariantCulture) : "0")}");

        using var memStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(memStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            var dataEntry = archive.CreateEntry("requests.csv");
            using (var entryStream = dataEntry.Open())
            using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
            {
                await streamWriter.WriteAsync(sbData.ToString());
            }

            var statsEntry = archive.CreateEntry("requests_stats.csv");
            using (var statsStream = statsEntry.Open())
            using (var statsWriter = new StreamWriter(statsStream, Encoding.UTF8))
            {
                await statsWriter.WriteAsync(sbStats.ToString());
            }
        }

        memStream.Position = 0;

        return File(memStream.ToArray(), "application/zip", "requests_report.zip");
    }
}