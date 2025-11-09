using LoginAndCrud.Application;
using LoginAndCrud.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoginAndCrud.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Company")]
public class EmployeesController(IEmployeeService svc) : ControllerBase
{
    private string Actor => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    public async Task<ActionResult<PagedEmployeesResponse>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
        => Ok(await svc.GetPagedAsync(page, pageSize, search, CurrentUserId, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeResponse>> GetById(int id, CancellationToken ct)
    {
        var emp = await svc.GetByIdAsync(id, CurrentUserId, ct);
        return emp is null ? NotFound() : Ok(emp);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeResponse>> Create([FromBody] CreateEmployeeRequest req, CancellationToken ct)
        => Ok(await svc.CreateAsync(req, Actor, CurrentUserId, ct));

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<EmployeeResponse>> Update(int id, [FromBody] UpdateUserRequest req, CancellationToken ct)
        => Ok(await svc.UpdateAsync(id, req, Actor, CurrentUserId, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await svc.DeleteAsync(id, CurrentUserId, ct);
        return NoContent();
    }
}
