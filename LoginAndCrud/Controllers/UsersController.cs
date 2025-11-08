using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LoginAndCrud.Application;
using LoginAndCrud.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace LoginAndCrud.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize] // requiere JWT
public class UsersController(IUserService svc) : ControllerBase
{
    private string Actor => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private bool IsAdmin => User.IsInRole("Admin");

    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedUsersResponse>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
        => Ok(await svc.GetPagedAsync(page, pageSize, search, ct));

    // GET /api/users/5
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken ct)
    {
        var u = await svc.GetByIdAsync(id, ct);
        return u is null ? NotFound() : Ok(u);
    }

    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest req, CancellationToken ct)
        => Ok(await svc.CreateAsync(req, Actor, ct));

    
    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserResponse>> Update(int id, [FromBody] UpdateUserRequest req, CancellationToken ct)
        => Ok(await svc.UpdateAsync(id, req, Actor, ct));

    
    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> ChangeMyPassword(int id, [FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        if (!IsAdmin && id != CurrentUserId) return Forbid();
        await svc.ChangePasswordAsync(id, req.CurrentPassword, req.NewPassword, Actor, bypassCurrent: IsAdmin, ct);
        return NoContent();
    }

    
    [HttpPut("{id:int}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        await svc.ChangePasswordAsync(id, currentPassword: "", newPassword: req.NewPassword, Actor, bypassCurrent: true, ct);
        return NoContent();
    }

     
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await svc.DeleteAsync(id, ct);
        return NoContent();
    }
}
