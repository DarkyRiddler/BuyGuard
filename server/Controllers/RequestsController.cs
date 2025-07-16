using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.DTOs.Request;
using server.Data;
using server.Models;

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
    public IActionResult GetRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();

        IQueryable<object> requestsQuery;

        if (userRole == "admin")
        {
            requestsQuery = _db.Request
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
                });
        }
        else if (userRole == "manager")
        {
            requestsQuery = _db.Request
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
                });
        }
        else if (userRole == "employee")
        {
            requestsQuery = _db.Request
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
                });
        }
        else
        {
            return Ok(new
            {
                request = new List<object>(),
                totalPages = 0,
                currentPage = page,
                totalRequests = 0
            });
        }

        var totalRequests = requestsQuery.Count();
        var totalPages = (int)Math.Ceiling((double)totalRequests / pageSize);
        var pagedRequests = requestsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            request = pagedRequests,
            totalPages = totalPages,
            currentPage = page,
            totalRequests = totalRequests
        });
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
    [HttpPost]
    public IActionResult CreateRequest([FromBody] CreateRequest request)
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();
        if (currentUserRole == "employee")
        {
            var matchingManager = _db.User
                .Where(u => u.Role == "manager" && u.ManagerLimitPln >= request.amount_pln)
                .OrderBy(u => u.ManagerLimitPln)
                .FirstOrDefault();

            int assignedManagerId;

            if (matchingManager != null)
            {
                assignedManagerId = matchingManager.Id;
            }
            else
            {
                var admin = _db.User.FirstOrDefault(u => u.Role == "admin");
                if (admin == null)
                    return BadRequest("Brak dostępnego administratora.");
                assignedManagerId = admin.Id;
            }

            var newRequest = new Request
            {
                Title = request.title,
                Description = request.description,
                AmountPln = request.amount_pln,
                Reason = request.reason,
                UserId = clientId,
                Status = "czeka", // czeka, potwierdzono, odrzucono, zakupione.
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Attachments = new List<Attachment>(),
                Notes = new List<Note>()
            };

            _db.Request.Add(newRequest);
            _db.SaveChanges();

            return Ok(new { success = true, requestId = newRequest.Id });
        }
        return Forbid("Tylko pracownicy moga skladac wnioski.");
    }
        
    [HttpPut("{id}")]
    public IActionResult UpdateRequest(int id, [FromBody] UpdateRequest updateDto)
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();
        if (userRole != "employee")
            return Forbid("Tylko pracowniy mogą edytować zgłoszenia");
        var request = _db.Request.FirstOrDefault(r => r.Id == id);
        if (request == null)
            return NotFound("Nie odnaleziono zgłoszenia");
        if (request.UserId != clientId)
            return Forbid("Możesz edytować tylko swoje zgłoszenia");
        if (request.Status != "czeka")
            return BadRequest("Możesz edytować tylko oczekujące zgłoszenia:");
        if (updateDto.AmountPln.HasValue)
        {
            if (updateDto.AmountPln.Value <= 0)
                return BadRequest("Kwota musi być większa od 0");
    
            if (updateDto.AmountPln.Value > 100000)
                return BadRequest("hola hola kolego to chyba trochę za dużo");
        }
        if (!string.IsNullOrEmpty(updateDto.Title))
            request.Title = updateDto.Title;
        if (!string.IsNullOrEmpty(updateDto.Description))
            request.Description = updateDto.Description;
        if (updateDto.AmountPln.HasValue)
            request.AmountPln = updateDto.AmountPln.Value;
        if (!string.IsNullOrEmpty(updateDto.Reason))
            request.Reason = updateDto.Reason;
        request.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        var updatedRequest = new
        {
            id = request.Id,
            title = request.Title,
            description = request.Description,
            amountPln = request.AmountPln,
            reason = request.Reason,
            status = request.Status,
            updatedAt = request.UpdatedAt
        };
        return Ok(updatedRequest);
    }
}