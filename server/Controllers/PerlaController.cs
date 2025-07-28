using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.Data;
using System.Text;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using server.Models;

// TODO osobno zrobić monthly i osobno zrobić ogólny 

//
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
    /*
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
    */
    [HttpGet("export")]
    public async Task<IActionResult> ExportRequests(
    [FromQuery] string? status = null,
    [FromQuery] decimal? minAmount = null,
    [FromQuery] decimal? maxAmount = null,
    [FromQuery] DateTime? dateFrom = null,
    [FromQuery] DateTime? dateTo = null,
    [FromQuery] string? searchName = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortOrder = "asc")
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();

        IQueryable<Request> requestsQuery;

        if (userRole == "admin")
        {
            requestsQuery = _db.Request.Include(r => r.User).Include(r => r.Manager);
        }
        else if (userRole == "manager")
        {
            requestsQuery = _db.Request
                .Include(r => r.User)
                .Include(r => r.Manager)
                .Where(r => r.ManagerId == clientId);
        }
        else if (userRole == "employee")
        {
            requestsQuery = _db.Request
                .Include(r => r.User)
                .Include(r => r.Manager)
                .Where(r => r.UserId == clientId);
        }
        else return Forbid();

        if (!string.IsNullOrEmpty(status))
            requestsQuery = requestsQuery.Where(r => r.Status == status);
        if (minAmount.HasValue)
            requestsQuery = requestsQuery.Where(r => r.AmountPln >= minAmount.Value);
        if (maxAmount.HasValue)
            requestsQuery = requestsQuery.Where(r => r.AmountPln <= maxAmount.Value);
        if (dateFrom.HasValue)
            requestsQuery = requestsQuery.Where(r => r.CreatedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            requestsQuery = requestsQuery.Where(r => r.CreatedAt <= dateTo.Value);
        if (!string.IsNullOrEmpty(searchName))
            requestsQuery = requestsQuery.Where(r =>
                r.User != null &&
                (r.User.FirstName.Contains(searchName) ||
                r.User.LastName.Contains(searchName) ||
                r.User.Email.Contains(searchName)));

        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "amount":
                    requestsQuery = sortOrder?.ToLower() == "desc"
                        ? requestsQuery.OrderByDescending(r => r.AmountPln)
                        : requestsQuery.OrderBy(r => r.AmountPln);
                    break;
                case "createdat":
                    requestsQuery = sortOrder?.ToLower() == "desc"
                        ? requestsQuery.OrderByDescending(r => r.CreatedAt)
                        : requestsQuery.OrderBy(r => r.CreatedAt);
                    break;
                case "status":
                    requestsQuery = sortOrder?.ToLower() == "desc"
                        ? requestsQuery.OrderByDescending(r => r.Status)
                        : requestsQuery.OrderBy(r => r.Status);
                    break;
                case "username":
                    requestsQuery = sortOrder?.ToLower() == "desc"
                        ? requestsQuery.OrderByDescending(r => r.User.FirstName).ThenByDescending(r => r.User.LastName)
                        : requestsQuery.OrderBy(r => r.User.FirstName).ThenBy(r => r.User.LastName);
                    break;
                case "aiscore":
                    requestsQuery = sortOrder?.ToLower() == "desc"
                        ? requestsQuery.OrderByDescending(r => r.AiScore)
                        : requestsQuery.OrderBy(r => r.AiScore);
                    break;
                default:
                    requestsQuery = requestsQuery.OrderByDescending(r => r.CreatedAt);
                    break;
            }
        }
        else
        {
            requestsQuery = requestsQuery.OrderByDescending(r => r.CreatedAt);
        }

        var requestsList = await requestsQuery
            .Select(r => new
            {
                r.Id,
                r.Status,
                r.Title,
                r.Description,
                r.AmountPln,
                r.Reason,
                r.AiScore,
                r.CreatedAt,
                r.UpdatedAt,
                r.UserId,
                UserName = r.User.FirstName + " " + r.User.LastName,
                r.User.Email,
                r.ManagerId,
                ManagerName = r.Manager != null ? r.Manager.FirstName + " " + r.Manager.LastName : "",
                ManagerEmail = r.Manager != null ? r.Manager.Email : ""
            })
            .ToListAsync();

        var sep = ";";
        var sbData = new StringBuilder();
        sbData.AppendLine(string.Join(sep, "Id", "Status", "Title", "Description", "AmountPln", "Reason",
            "AiScore", "CreatedAt", "UpdatedAt", "UserId", "UserName",
            "UserEmail", "ManagerId", "ManagerName", "ManagerEmail"));

        foreach (var r in requestsList)
        {
            sbData.AppendLine(string.Join(sep, r.Id, r.Status, r.Title, r.Description, r.AmountPln, r.Reason,
                r.AiScore, r.CreatedAt.ToString("dd-MM-yyyy HH:mm:ss"),
                r.UpdatedAt?.ToString("dd-MM-yyyy HH:mm:ss") ?? "",
                r.UserId, r.UserName, r.Email,
                r.ManagerId, r.ManagerName, r.ManagerEmail));
        }

        var sbStats = new StringBuilder();
        sbStats.AppendLine("Statystyka,Wartość");
        sbStats.AppendLine($"Liczba zgłoszeń,{requestsList.Count}");
        sbStats.AppendLine($"Suma kwot,{requestsList.Sum(r => r.AmountPln).ToString(CultureInfo.InvariantCulture)}");
        sbStats.AppendLine($"Średnia kwota,{(requestsList.Count > 0 ? requestsList.Average(r => r.AmountPln).ToString(CultureInfo.InvariantCulture) : "0")}");

        using var memStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(memStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            var dataEntry = archive.CreateEntry("requests.csv");
            using (var entryStream = dataEntry.Open())
            using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
                await streamWriter.WriteAsync(sbData.ToString());

            var statsEntry = archive.CreateEntry("requests_stats.csv");
            using (var statsStream = statsEntry.Open())
            using (var statsWriter = new StreamWriter(statsStream, Encoding.UTF8))
                await statsWriter.WriteAsync(sbStats.ToString());
        }

        memStream.Position = 0;
        return File(memStream.ToArray(), "application/zip", "requests_report.zip");
    }

}