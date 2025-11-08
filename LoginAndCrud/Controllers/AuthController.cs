using Microsoft.AspNetCore.Mvc;
using LoginAndCrud.Contracts;
using LoginAndCrud.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace LoginAndCrud.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        try { return Ok(await auth.RegisterAsync(req, ct)); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        try { return Ok(await auth.LoginAsync(req, ct)); }
        catch (Exception ex) { return Unauthorized(new { message = ex.Message }); }
    }

    
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        userId = User.Identity?.Name,
        name = User.Identity?.Name,
        roles = User.Claims.Where(c => c.Type.EndsWith("/role") || c.Type == "role").Select(c => c.Value)
    });
}
