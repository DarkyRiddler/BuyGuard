using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using server.Data;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public RequestsController(ApplicationDbContext db)
    {
        this._db = db;
    }

    [HttpGet]
    public IActionResult GetRequests()
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();
        if (userRole == "admin")
        {
            var requests = _db.Request
                .Select(r => new
                {
                    id = r.Id,
                    userEmail = r.User.Email,
                    userId = r.UserId,
                    userName = $"{r.User.FirstName} {r.User.LastName}",
                    managerId = r.ManagerId,
                    managerName = r.Manager != null ? $"{r.Manager.FirstName} {r.Manager.LastName}" : null,
                    description = r.Description,
                    amountPln = r.AmountPln,
                    reason = r.Reason,
                    status = r.Status,
                    createdAt = r.CreatedAt,
                    updatedAt = r.UpdatedAt,
                }).ToList();
            return Ok(new { request = requests });
        }
        else if (userRole == "manager")
        {
            var requests = _db.Request
                .Where(r => r.ManagerId == clientId)
                .Select(r => new
                {
                    id = r.Id,
                    userEmail = r.User.Email,
                    userId = r.UserId,
                    userName = $"{r.User.FirstName} {r.User.LastName}",
                    managerId = r.ManagerId,
                    managerName = r.Manager != null ? $"{r.Manager.FirstName} {r.Manager.LastName}" : null,
                    description = r.Description,
                    amountPln = r.AmountPln,
                    reason = r.Reason,
                    status = r.Status,
                    createdAt = r.CreatedAt,
                    updatedAt = r.UpdatedAt,
                }).ToList();
            return Ok(new { user = requests });
        }
        else if (userRole == "employee")
        {
            var requests = _db.Request
                .Where(r => r.UserId == clientId)
                .Select(r => new
                {
                    id = r.Id,
                    userEmail = r.User.Email,
                    userId = r.UserId,
                    userName = $"{r.User.FirstName} {r.User.LastName}",
                    managerId = r.ManagerId,
                    managerName = r.Manager != null ? $"{r.Manager.FirstName} {r.Manager.LastName}" : null,
                    description = r.Description,
                    amountPln = r.AmountPln,
                    reason = r.Reason,
                    status = r.Status,
                    createdAt = r.CreatedAt,
                    updatedAt = r.UpdatedAt,
                }).ToList();
            return Ok(new { user = requests });
        }
        return Ok(new List<object>());
    }
    [HttpGet("{id}")]
    public IActionResult GetSpecificRequest(int id)
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();

        var request = _db.Request
            .Where(r => r.Id == id)
            .Select(r => new
            {
                id = r.Id,
                title = r.Title,
                description = r.Description,
                amountPln = r.AmountPln,
                reason = r.Reason,
                status = r.Status,
                aiScore = r.AiScore,
                createdAt = r.CreatedAt,
                updatedAt = r.UpdatedAt,
                userId = r.UserId,
                userName = $"{r.User.FirstName} {r.User.LastName}",
                userEmail = r.User.Email,
                managerId = r.ManagerId,
                managerName = r.Manager != null ? $"{r.Manager.FirstName} {r.Manager.LastName}" : null,
                managerEmail = r.Manager != null ? r.Manager.Email : null,
                attachments = r.Attachments.Select(a => new { a.FileUrl, a.MimeType }),
                notes = r.Notes.Select(n => new
                {
                    authorName = n.Author != null ? $"{n.Author.FirstName} {n.Author.LastName}" : null,
                    n.Body,
                    n.CreatedAt
                })
            })
            .FirstOrDefault();
        if (userRole == "admin" || request.userId == clientId || request.managerId == clientId)
            return Ok(request);
        return Forbid();
    }
}