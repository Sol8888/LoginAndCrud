using LoginAndCrud.Application;
using LoginAndCrud.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoginAndCrud.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController(IReservationService svc) : ControllerBase
{
  
    private string Actor => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private string Role => User.FindFirstValue(ClaimTypes.Role) ?? "";

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ReservationResponse>> Create([FromBody] CreateReservationRequest req, CancellationToken ct)
        => Ok(await svc.CreateAsync(req, CurrentUserId, Actor, ct));

    [HttpGet("Mine")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedReservationsResponse>> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await svc.GetMyReservationsAsync(CurrentUserId, page, pageSize, ct));

    [HttpGet("by-user/{userId:int}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<ActionResult<PagedReservationsResponse>> GetByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await svc.GetByUserAsync(userId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("by-company/{companyId:int?}")]
    [Authorize(Roles = "Admin,Company,Employee")]
    public async Task<ActionResult<PagedReservationsResponse>> GetByCompany(
     int? companyId,
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 20,
     CancellationToken ct = default)
    {
        if (Role is "Company" or "Employee")
        {
            if (Role == "Employee" || companyId is null or <= 0)
            {
                companyId = await svc.GetCompanyIdForUserAsync(CurrentUserId, ct);
            }
        }

        if (companyId is null or <= 0)
        {
            return BadRequest("Debe proporcionar un companyId válido.");
        }

        var result = await svc.GetByCompanyAsync(companyId.Value, page, pageSize, ct);
        return Ok(result);
    }



    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedReservationsResponse>> GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
    => Ok(await svc.GetAllAsync(page, pageSize, ct));

    [HttpPut("{id}/status")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ReservationResponse>> UpdateStatus(int id, [FromBody] bool isDone, CancellationToken ct)
    {
        var result = await svc.UpdateStatusAsync(id, isDone, ct);
        return Ok(result);
    }



}
