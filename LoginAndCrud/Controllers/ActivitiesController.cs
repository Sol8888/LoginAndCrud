using System.Security.Claims;
using LoginAndCrud.Application;
using LoginAndCrud.Contracts;
using LoginAndCrud.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoginAndCrud.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController(IActivityService svc) : ControllerBase
{
    private string Actor => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private string Role => User.FindFirstValue(ClaimTypes.Role) ?? "";

    [HttpGet]
    [Authorize(Roles = "Admin,Company,Employee")]
    public async Task<ActionResult<PagedActivitiesResponse>> Get(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null,
    CancellationToken ct = default)
    {
        return Ok(await svc.GetPagedAsync(page, pageSize, search, Role, CurrentUserId, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Company,Employee")]
    public async Task<ActionResult<ActivityResponse>> Create([FromBody] CreateActivityRequest req, CancellationToken ct)
    => Ok(await svc.CreateAsync(req, CurrentUserId, Actor, ct));

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Company,Employee")]
    public async Task<ActionResult<ActivityResponse>> Update(int id, [FromBody] UpdateActivityRequest req, CancellationToken ct)
        => Ok(await svc.UpdateAsync(id, req, CurrentUserId, Actor, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Company,Employee")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await svc.DeleteAsync(id, CurrentUserId, ct);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Company,Employee")]
    public async Task<ActionResult<ActivityResponse>> GetById(int id, CancellationToken ct)
    {
        var a = await svc.GetByIdAsync(id, CurrentUserId, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpGet("all")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<List<ActivityResponse>>> GetAll(CancellationToken ct)
    {
        return Ok(await svc.GetAllAsync(CurrentUserId, ct));
    }
}
