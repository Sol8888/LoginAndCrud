using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using Microsoft.EntityFrameworkCore;
using LoginAndCrud.Infrastructure.Security;

namespace LoginAndCrud.Application;

public interface IActivityCategoryService
{
    Task AddCategoryAsync(int activityId, int categoryId, int userId, string role, string actor, CancellationToken ct);
    Task RemoveCategoryAsync(int activityId, int categoryId, int userId, string role, CancellationToken ct);
}

public class ActivityCategoryService(AppDbContext db) : IActivityCategoryService
{
    public async Task AddCategoryAsync(int activityId, int categoryId, int userId, string role, string actor, CancellationToken ct)
    {
        var activity = await GetActivityByRoleAsync(activityId, userId, role, ct)
            ?? throw new InvalidOperationException("Actividad no encontrada.");

        if (!await db.Categories.AnyAsync(c => c.Id == categoryId, ct))
            throw new InvalidOperationException("Categoría no encontrada.");

        var exists = await db.ActivityCategories.AnyAsync(ac => ac.ActivityId == activityId && ac.CategoryId == categoryId, ct);
        if (exists) return;

        var ac = new ActivityCategory
        {
            ActivityId = activityId,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor
        };

        db.ActivityCategories.Add(ac);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveCategoryAsync(int activityId, int categoryId, int userId, string role, CancellationToken ct)
    {
        var activity = await GetActivityByRoleAsync(activityId, userId, role, ct)
            ?? throw new InvalidOperationException("Actividad no encontrada.");

        var ac = await db.ActivityCategories
            .FirstOrDefaultAsync(x => x.ActivityId == activityId && x.CategoryId == categoryId, ct);

        if (ac is null) return;

        db.ActivityCategories.Remove(ac);
        await db.SaveChangesAsync(ct);
    }

    private async Task<Activity?> GetActivityByRoleAsync(int activityId, int userId, string role, CancellationToken ct)
    {
        return role switch
        {
            "Admin" => await db.Activities
                .FirstOrDefaultAsync(a => a.Id == activityId, ct),

            "Company" => await db.Activities
                .Include(a => a.Company) // Asegúrate de incluir Company
                .FirstOrDefaultAsync(a => a.Id == activityId && a.Company.OwnerUserId == userId, ct),

            "Employee" => await db.Activities
                .Include(a => a.Company).ThenInclude(c => c.Employees) // ✅ Incluye empleados de la empresa
                .Where(a => a.Company.Employees.Any(ec => ec.UserId == userId))
                .FirstOrDefaultAsync(a => a.Id == activityId, ct),

            _ => null
        };
    }
}

