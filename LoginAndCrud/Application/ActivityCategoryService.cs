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
    Task<List<ActivityWithCategoriesResponse>> GetActivitiesWithCategoriesByCompanyAsync(int userId, string role, CancellationToken ct);
    Task<ActivityWithCategoriesResponse?> GetActivityWithCategoriesByIdAsync(int activityId, int userId, string role, CancellationToken ct);

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

    public async Task<List<ActivityWithCategoriesResponse>> GetActivitiesWithCategoriesByCompanyAsync(int userId, string role, CancellationToken ct)
    {
        IQueryable<Activity> query = db.Activities
            .Include(a => a.ActivityCategories)
                .ThenInclude(ac => ac.Category)
            .Include(a => a.Company);

        query = role switch
        {
            "Admin" => query,
            "Company" => query.Where(a => a.Company.OwnerUserId == userId),
            "Employee" => query.Where(a => a.Company.Employees.Any(e => e.UserId == userId)),
            _ => Enumerable.Empty<Activity>().AsQueryable()
        };

        var activities = await query.ToListAsync(ct);

        return activities.Select(a => new ActivityWithCategoriesResponse
        {
            ActivityId = a.Id,
            Title = a.Title,
            Categories = a.ActivityCategories
                .Where(ac => ac.Category != null)
                .Select(ac => new CategoryDto
                {
                    Id = ac.Category!.Id,
                    Name = ac.Category.Name
                }).ToList()
        }).ToList();
    }

    public async Task<ActivityWithCategoriesResponse?> GetActivityWithCategoriesByIdAsync(int activityId, int userId, string role, CancellationToken ct)
    {
        IQueryable<Activity> query = db.Activities
            .Include(a => a.ActivityCategories)
                .ThenInclude(ac => ac.Category)
            .Include(a => a.Company);

        query = role switch
        {
            "Admin" => query,
            "Company" => query.Where(a => a.Company.OwnerUserId == userId),
            "Employee" => query.Where(a => a.Company.Employees.Any(e => e.UserId == userId)),
            _ => Enumerable.Empty<Activity>().AsQueryable()
        };

        var activity = await query.FirstOrDefaultAsync(a => a.Id == activityId, ct);
        if (activity is null) return null;

        return new ActivityWithCategoriesResponse
        {
            ActivityId = activity.Id,
            Title = activity.Title,
            Categories = activity.ActivityCategories
                .Where(ac => ac.Category != null)
                .Select(ac => new CategoryDto
                {
                    Id = ac.Category!.Id,
                    Name = ac.Category.Name
                }).ToList()
        };
    }


}

