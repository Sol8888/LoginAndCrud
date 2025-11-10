using LoginAndCrud.Application;
using LoginAndCrud.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace LoginAndCrud.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CategoriesController(ICategoryService svc) : ControllerBase
{
    private string Actor => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private string Role => User.FindFirstValue(ClaimTypes.Role) ?? "";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll(CancellationToken ct)
        => Ok(await svc.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id, CancellationToken ct)
    {
        var c = await svc.GetByIdAsync(id, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create([FromBody] CreateCategoryRequest req, CancellationToken ct)
        => Ok(await svc.CreateAsync(req, Actor, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, [FromBody] UpdateCategoryRequest req, CancellationToken ct)
        => Ok(await svc.UpdateAsync(id, req, Actor, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await svc.DeleteAsync(id, ct);
        return NoContent();
    }
}
