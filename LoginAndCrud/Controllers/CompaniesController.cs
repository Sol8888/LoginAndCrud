using LoginAndCrud.Application;
using LoginAndCrud.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace LoginAndCrud.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController(ICompanyService svc) : ControllerBase
{
    private string Actor => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private string Role => User.FindFirstValue(ClaimTypes.Role) ?? "";

    [HttpGet]
    [Authorize(Roles = "Admin,Company")]
    public async Task<ActionResult<PagedCompaniesResponse>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
        => Ok(await svc.GetPagedAsync(page, pageSize, search, Role, CurrentUserId, ct));

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<ActionResult<CompanyResponse>> GetById(int id, CancellationToken ct)
    {
        var c = await svc.GetByIdAsync(id, Role, CurrentUserId, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CreateCompanyRequest req, CancellationToken ct)
        => Ok(await svc.CreateAsync(req, Actor, ct));

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CompanyResponse>> Update(int id, [FromBody] UpdateCompanyRequest req, CancellationToken ct)
        => Ok(await svc.UpdateAsync(id, req, Actor, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await svc.DeleteAsync(id, Actor, ct);
        return NoContent();
    }
}