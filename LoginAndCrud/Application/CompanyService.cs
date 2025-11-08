using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using LoginAndCrud.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace LoginAndCrud.Application;

public interface ICompanyService
{
    Task<PagedCompaniesResponse> GetPagedAsync(int page, int pageSize, string? search, string actorRole, int actorId, CancellationToken ct);
    Task<CompanyResponse?> GetByIdAsync(int id, string actorRole, int actorId, CancellationToken ct);
    Task<CompanyResponse> CreateAsync(CreateCompanyRequest req, string actor, CancellationToken ct);
    Task<CompanyResponse> UpdateAsync(int id, UpdateCompanyRequest req, string actor, CancellationToken ct);
    Task DeleteAsync(int id, string actor, CancellationToken ct);
}

public class CompanyService(AppDbContext db) : ICompanyService
{
    private static CompanyResponse Map(Company c) =>
        new(c.Id, c.Name, c.Description, c.OwnerUserId, c.AvgRating, c.RatingCount,c.IsActive, c.CreatedAt, c.UpdatedAt, c.CreatedBy, c.UpdatedBy);

    public async Task<PagedCompaniesResponse> GetPagedAsync(int page, int pageSize, string? search, string actorRole, int actorId, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 200 ? 20 : pageSize;

        var query = db.Companies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || c.Description.Contains(search));

        if (actorRole == "Company")
            query = query.Where(c => c.OwnerUserId == actorId);

        if (actorRole != "Admin" && actorRole != "Company")
            throw new UnauthorizedAccessException("No tienes permiso para ver empresas.");

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => Map(c))
            .ToListAsync(ct);

        return new(page, pageSize, total, items);
    }

    public async Task<CompanyResponse?> GetByIdAsync(int id, string actorRole, int actorId, CancellationToken ct)
    {
        var c = await db.Companies.FindAsync([id], ct);
        if (c is null) return null;

        if (actorRole == "Admin" || (actorRole == "Company" && c.OwnerUserId == actorId))
            return Map(c);

        throw new UnauthorizedAccessException("No tienes permiso para ver esta empresa.");
    }

    public async Task<CompanyResponse> CreateAsync(CreateCompanyRequest req, string actor, CancellationToken ct)
    {
        var owner = await db.Users.FindAsync([req.OwnerUserId], ct)
                    ?? throw new KeyNotFoundException("Usuario propietario no encontrado.");

        if (owner.Role != "Company")
            throw new InvalidOperationException("El propietario debe tener rol 'Company'.");

        var c = new Company
        {
            Name = req.Name,
            Description = req.Description,
            OwnerUserId = req.OwnerUserId,
            AvgRating = 0,
            RatingCount = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor
        };

        db.Companies.Add(c);
        await db.SaveChangesAsync(ct);
        return Map(c);
    }

    public async Task<CompanyResponse> UpdateAsync(int id, UpdateCompanyRequest req, string actor, CancellationToken ct)
    {
        var c = await db.Companies.FindAsync([id], ct) ?? throw new KeyNotFoundException("Empresa no encontrada.");


        if (!string.IsNullOrWhiteSpace(req.Name)) c.Name = req.Name!;
        if (!string.IsNullOrWhiteSpace(req.Description)) c.Description = req.Description!;
        if (req.IsActive.HasValue) c.IsActive = req.IsActive.Value;

        c.UpdatedAt = DateTime.UtcNow;
        c.UpdatedBy = actor;

        await db.SaveChangesAsync(ct);
        return Map(c);
    }

    public async Task DeleteAsync(int id, string actor, CancellationToken ct)
    {
        var c = await db.Companies.FindAsync([id], ct) ?? throw new KeyNotFoundException("Empresa no encontrada.");
        db.Companies.Remove(c);
        await db.SaveChangesAsync(ct);
    }
}
