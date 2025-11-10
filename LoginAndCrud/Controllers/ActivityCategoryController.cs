using Microsoft.AspNetCore.Mvc;
using LoginAndCrud.Contracts;
using LoginAndCrud.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace LoginAndCrud.Controllers;

[ApiController]
[Route("api/activities/{activityId:int}/categories")]
[Authorize]
public class ActivityCategoryController(IActivityCategoryService svc) : ControllerBase
{
    private string Actor => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private string Role => User.FindFirstValue(ClaimTypes.Role) ?? "";

    [HttpPost]
    [Authorize(Roles = "Admin,Company,Employee")]
    public async Task<IActionResult> Add(int activityId, [FromBody] AddCategoryToActivityRequest req, CancellationToken ct)
    {
        await svc.AddCategoryAsync(activityId, req.CategoryId, CurrentUserId, Role, Actor, ct);
        return NoContent();
    }

    [HttpDelete("{categoryId:int}")]
    [Authorize(Roles = "Admin,Company,Employee")]
    public async Task<IActionResult> Remove(int activityId, int categoryId, CancellationToken ct)
    {
        await svc.RemoveCategoryAsync(activityId, categoryId, CurrentUserId, Role, ct);
        return NoContent();
    }
}
