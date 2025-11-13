using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using Microsoft.EntityFrameworkCore;
using LoginAndCrud.Infrastructure.Security;

namespace LoginAndCrud.Application;

public interface IActivityService
{
    Task<List<ActivityResponse>> GetAllAsync(int userId, CancellationToken ct);
    Task<ActivityResponse?> GetByIdAsync(int id, int userId, CancellationToken ct);
    Task<ActivityResponse> CreateAsync(CreateActivityRequest req, int userId, string actor, CancellationToken ct);
    Task<ActivityResponse> UpdateAsync(int id, UpdateActivityRequest req, int userId, string actor, CancellationToken ct);
    Task DeleteAsync(int id, int userId, CancellationToken ct);
    Task<PagedActivitiesResponse> GetPagedAsync(int page, int pageSize, string? search, string role, int userId, CancellationToken ct);


}

public class ActivityService(AppDbContext db) : IActivityService
{
    
    private static ActivityResponse Map(Activity a) =>
    new(
        a.Id,
        a.CompanyId,
        a.Title,
        a.Description,
        a.LocationText,
        a.Latitude,
        a.Longitude,
        a.StartAt,
        a.EndAt,
        a.Capacity,
        a.Price,
        a.Currency,
        a.Status,
        a.AllowWaitlist,
        a.AvgRating,
        a.RatingCount,
        a.CreatedAt,
        a.UpdatedAt,
        a.CreatedBy,
        a.UpdatedBy
    );

    private async Task<int> GetCompanyIdForUserAsync(int userId, CancellationToken ct)
    {
        var companyId = await db.Companies
            .Where(c => c.OwnerUserId == userId || db.EmployeeCompany.Any(ec => ec.UserId == userId && ec.CompanyId == c.Id))
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);

        if (companyId == 0)
            throw new InvalidOperationException("El usuario no está asociado a una empresa.");

        return companyId;
    }

    public async Task<PagedActivitiesResponse> GetPagedAsync(int page, int pageSize, string? search, string role, int userId, CancellationToken ct)
    {
        var query = db.Activities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Title.Contains(search));

        if (role != "Admin")
        {
            int companyId = 0;

            if (role == "Company")
            {
                companyId = await db.Companies
                    .Where(c => c.OwnerUserId == userId)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(ct);
            }
            else if (role == "Employee")
            {
                companyId = await db.EmployeeCompany
                    .Where(e => e.UserId == userId)
                    .Select(e => e.CompanyId)
                    .FirstOrDefaultAsync(ct);
            }

            if (companyId == 0)
                throw new InvalidOperationException("El usuario no está asociado a una empresa.");

            query = query.Where(a => a.CompanyId == companyId);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ActivityResponse(
                a.Id,
                a.CompanyId,
                a.Title,
                a.Description,
                a.LocationText,
                a.Latitude,
                a.Longitude,
                a.StartAt,
                a.EndAt,
                a.Capacity,
                a.Price,
                a.Currency,
                a.Status,
                a.AllowWaitlist,
                a.AvgRating,
                a.RatingCount,
                a.CreatedAt,
                a.UpdatedAt,
                a.CreatedBy,
                a.UpdatedBy
            ))
            .ToListAsync(ct);

        return new PagedActivitiesResponse(page, pageSize, total, items);
    }
    public async Task<List<ActivityResponse>> GetAllAsync(int userId, CancellationToken ct)
    {
        var companyId = await GetCompanyIdForUserAsync(userId, ct);
        var list = await db.Activities
            .Where(a => a.CompanyId == companyId)
            .ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    public async Task<ActivityResponse?> GetByIdAsync(int id, int userId, CancellationToken ct)
    {
        var companyId = await GetCompanyIdForUserAsync(userId, ct);
        var a = await db.Activities
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct);

        return a is null ? null : Map(a);
    }

    public async Task<ActivityResponse> CreateAsync(CreateActivityRequest req, int userId, string actor, CancellationToken ct)
    {
        var companyId = await GetCompanyIdForUserAsync(userId, ct);

        var company = await db.Companies
            .Include(c => c.Employees)
            .FirstOrDefaultAsync(c => c.OwnerUserId == userId || c.Employees.Any(e => e.UserId == userId), ct)
            ?? throw new KeyNotFoundException("Empresa no encontrada.");



        var activity = new Activity
        {
            CompanyId = companyId,
            Title = req.Title,
            Description = req.Description,
            LocationText = req.LocationText,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            StartAt = req.StartAt,
            EndAt = req.EndAt,
            Capacity = req.Capacity,
            Price = req.Price,
            Currency = req.Currency,
            Status = req.Status,
            AllowWaitlist = req.AllowWaitlist,
            AvgRating = 0,
            RatingCount = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor
        };

        db.Activities.Add(activity);
        await db.SaveChangesAsync(ct);
        return Map(activity);
    }

    public async Task<ActivityResponse> UpdateAsync(int id, UpdateActivityRequest req, int userId, string actor, CancellationToken ct)
    {
        var activity = await db.Activities.FindAsync([id], ct) ?? throw new KeyNotFoundException("Actividad no encontrada.");
        
        activity.Title = req.Title;
        activity.Description = req.Description;
        activity.LocationText = req.LocationText;
        activity.Latitude = req.Latitude;
        activity.Longitude = req.Longitude;
        activity.StartAt = req.StartAt;
        activity.EndAt = req.EndAt;
        activity.Capacity = req.Capacity;
        activity.Price = req.Price;
        activity.Currency = req.Currency;
        activity.Status = req.Status;
        activity.AllowWaitlist = req.AllowWaitlist;
        activity.UpdatedAt = DateTime.UtcNow;
        activity.UpdatedBy = actor;

        await db.SaveChangesAsync(ct);

        return Map(activity);
    }

    public async Task DeleteAsync(int id, int userId, CancellationToken ct)
    {
        var companyId = await GetCompanyIdForUserAsync(userId, ct);
        var a = await db.Activities
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct)
            ?? throw new KeyNotFoundException("Actividad no encontrada.");

        db.Activities.Remove(a);
        await db.SaveChangesAsync(ct);
    }
}
