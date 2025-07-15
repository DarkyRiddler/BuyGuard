using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using server.Data;


[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public RequestsController(ApplicationDbContext db)
    {
        this._db = db;
    }
    
}