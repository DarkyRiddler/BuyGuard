using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using server.DTOs.Request;
using server.Data;
using server.Models;
using Microsoft.EntityFrameworkCore;

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
    public IActionResult GetRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
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

        if (!string.IsNullOrEmpty(status))
        {
            requestsQuery = requestsQuery.Where(r => r.Status == status);
        }

        if (minAmount.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.AmountPln >= minAmount.Value);
        }

        if (maxAmount.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.AmountPln <= maxAmount.Value);
        }

        if (dateFrom.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.CreatedAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.CreatedAt <= dateTo.Value);
        }

        if (!string.IsNullOrEmpty(searchName))
        {
            requestsQuery = requestsQuery.Where(r =>
                r.User != null &&
                (r.User.FirstName.Contains(searchName) ||
                 r.User.LastName.Contains(searchName) ||
                 r.User.Email.Contains(searchName)));
        }

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
                default:
                    requestsQuery = requestsQuery.OrderByDescending(r => r.CreatedAt);
                    break;
            }
        }
        else
        {
            requestsQuery = requestsQuery.OrderByDescending(r => r.CreatedAt);
        }

        var totalRequests = requestsQuery.Count();
        var totalPages = (int)Math.Ceiling((double)totalRequests / pageSize);
        var pagedRequests = requestsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                id = r.Id,
                userEmail = r.User != null ? r.User.Email : null,
                userId = r.UserId,
                userName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : null,
                managerId = r.ManagerId,
                managerName = r.Manager != null ? $"{r.Manager.FirstName} {r.Manager.LastName}" : null,
                description = r.Description,
                url = r.Url,
                amountPln = r.AmountPln,
                reason = r.Reason,
                status = r.Status,
                createdAt = r.CreatedAt,
                updatedAt = r.UpdatedAt,
            })
            .ToList();
        return Ok(new
        {
            request = pagedRequests,
            totalPages = totalPages,
            currentPage = page,
            totalRequests = totalRequests,
            filters = new
            {
                status = status,
                minAmount = minAmount,
                maxAmount = maxAmount,
                dateFrom = dateFrom,
                dateTo = dateTo,
                searchName = searchName,
                sortBy = sortBy,
                sortOrder = sortOrder
            }
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
            .Include(r => r.User)
            .Include(r => r.Manager)
            .Include(r => r.Attachments)
            .Include(r => r.Notes)
                .ThenInclude(n => n.Author)
            .FirstOrDefault();

        if (request == null)
            return NotFound();

        if (userRole != "admin" && request.UserId != clientId && request.ManagerId != clientId)
            return Forbid();

        return Ok(new
        {
            id = request.Id,
            title = request.Title,
            description = request.Description,
            url = request.Url,
            amountPln = request.AmountPln,
            reason = request.Reason,
            status = request.Status,
            aiScore = request.AiScore,
            createdAt = request.CreatedAt,
            updatedAt = request.UpdatedAt,
            userId = request.UserId,
            userName = request.User != null ? $"{request.User.FirstName} {request.User.LastName}" : null,
            userEmail = request.User?.Email,
            managerId = request.ManagerId,
            managerName = request.Manager != null ? $"{request.Manager.FirstName} {request.Manager.LastName}" : null,
            managerEmail = request.Manager?.Email,
            attachments = request.Attachments?.Select(a => new { a.FileUrl, a.MimeType }).ToList(),
            notes = request.Notes?.Select(n => new
            {
                authorName = n.Author != null ? $"{n.Author.FirstName} {n.Author.LastName}" : null,
                n.Body,
                n.CreatedAt
            }).ToList()
        });
    }

    [HttpPost]
    public IActionResult CreateRequest([FromBody] CreateRequest request)
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();

        if (currentUserRole != "employee")
            return Forbid("Tylko pracownicy moga skladac wnioski.");
        
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
            Url = request.url,
            AmountPln = request.amount_pln,
            Reason = request.reason,
            UserId = clientId,
            ManagerId = assignedManagerId,
            Manager = _db.User.FirstOrDefault(u => u.Id == assignedManagerId) ?? throw new InvalidOperationException("Manager not found"),
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


    // private string GetMimeTypeFromUrl(string url)
    // {
    //     var extension = Path.GetExtension(url).ToLowerInvariant();
    //     return extension switch
    //     {
    //         ".pdf" => "application/pdf",
    //         ".jpg" => "image/jpeg",
    //         ".jpeg" => "image/jpeg",
    //         ".png" => "image/png",
    //         ".doc" => "application/msword",
    //         ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    //         ".xls" => "application/vnd.ms-excel",
    //         ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    //         ".txt" => "text/plain",
    //         _ => "application/octet-stream"
    //     };
    // }
    [HttpPatch("{id}/status")]
    public IActionResult UpdateRequestStatus(int id, [FromBody] UpdateRequestStatusDTO statusDto)
    {
        var clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            return Unauthorized();
        if (userRole != "manager" && userRole != "admin")
            return Forbid("Tylko menedżer lub admin może zmieniać status");
        var request = _db.Request.FirstOrDefault(r => r.Id == id);
        if (request == null)
            return NotFound("Zgłoszenie nie zostało znalezione");
        if (userRole == "manager" && request.ManagerId != clientId)
            return Forbid("Możesz zmieniać status tylko przypisanych do ciebie zgłoszeń");
        if (request.Status != "czeka")
            return BadRequest("Można zmienić status tylko oczekujących zgłoszeń");
        var validStatuses = new[] { "potwierdzono", "odrzucono", "zakupione" };
        if (!validStatuses.Contains(statusDto.Status))
            return BadRequest("Niepoprawny status");
        request.Status = statusDto.Status;
        request.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(statusDto.Reason))
        {
            var note = new server.Models.Note
            {
                RequestId = request.Id,
                AuthorId = clientId,
                Body = $"Status zmieniony na {statusDto.Status}. Powód: {statusDto.Reason}",
                CreatedAt = DateTime.UtcNow
            };
            _db.Note.Add(note);
        }

        _db.SaveChanges();
        return Ok(new
        {
            id = request.Id,
            status = request.Status,
            updatedAt = request.UpdatedAt,
            message = $"Status zgłoszenia został zmieniony na {statusDto.Status}"
        });

    }
}