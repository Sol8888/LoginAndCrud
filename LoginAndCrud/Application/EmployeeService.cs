using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using Microsoft.EntityFrameworkCore;
using LoginAndCrud.Infrastructure.Security;

namespace LoginAndCrud.Application;

public interface IEmployeeService
{
    Task<PagedEmployeesResponse> GetPagedAsync(int page, int pageSize, string? search, int ownerUserId, CancellationToken ct);
    Task<EmployeeResponse?> GetByIdAsync(int employeeId, int ownerUserId, CancellationToken ct);
    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest req, string actor, int ownerUserId, CancellationToken ct);
    Task<EmployeeResponse> UpdateAsync(int id, UpdateUserRequest req, string actor, int ownerUserId, CancellationToken ct);
    Task DeleteAsync(int id, int ownerUserId, CancellationToken ct);
}

public class EmployeeService(AppDbContext db) : IEmployeeService
{
    private static EmployeeResponse Map(User user, EmployeeCompany ec) =>
        new(user.Id, user.Username, user.Email, user.Role, user.IsActive, user.CreatedAt, user.UpdatedAt, user.CreatedBy, user.UpdatedBy, ec.RoleInCompany, ec.CompanyId);

    public async Task<PagedEmployeesResponse> GetPagedAsync(int page, int pageSize, string? search, int ownerUserId, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 200 ? 20 : pageSize;

        var company = await db.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId, ct)
            ?? throw new InvalidOperationException("Empresa no encontrada.");

        var query = db.EmployeeCompanies
            .Include(ec => ec.User)
            .Where(ec => ec.CompanyId == company.Id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(ec => ec.User.Username.Contains(search) || ec.User.Email.Contains(search));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(ec => ec.User.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new(page, pageSize, total, items.Select(ec => Map(ec.User, ec)));
    }

    public async Task<EmployeeResponse?> GetByIdAsync(int employeeId, int ownerUserId, CancellationToken ct)
    {
        var company = await db.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId, ct)
            ?? throw new InvalidOperationException("Empresa no encontrada.");

        var ec = await db.EmployeeCompanies
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == employeeId && x.CompanyId == company.Id, ct);

        return ec is null ? null : Map(ec.User, ec);
    }

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest req, string actor, int ownerUserId, CancellationToken ct)
    {
        var company = await db.Companies
            .FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId, ct)
            ?? throw new InvalidOperationException("Empresa no encontrada.");

        if (await db.Users.AnyAsync(x => x.Username == req.Username || x.Email == req.Email, ct))
            throw new InvalidOperationException("Username o Email ya existen.");

        var (hash, salt) = PasswordHasher.Hash(req.Password);
        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "Employee",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var ec = new EmployeeCompany
        {
            UserId = user.Id,
            CompanyId = company.Id,
            RoleInCompany = req.RoleInCompany ?? "Employee",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor
        };

        db.EmployeeCompanies.Add(ec);
        await db.SaveChangesAsync(ct);

        return Map(user, ec);
    }

    public async Task<EmployeeResponse> UpdateAsync(int id, UpdateUserRequest req, string actor, int ownerUserId, CancellationToken ct)
    {
        var company = await db.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId, ct)
            ?? throw new InvalidOperationException("Empresa no encontrada.");

        var ec = await db.EmployeeCompanies
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == id && x.CompanyId == company.Id, ct)
            ?? throw new KeyNotFoundException("Empleado no encontrado.");

        var user = ec.User;

        if (!string.IsNullOrWhiteSpace(req.Email) && !string.Equals(user.Email, req.Email, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await db.Users.AnyAsync(x => x.Email == req.Email && x.Id != id, ct);
            if (exists) throw new InvalidOperationException("Email ya en uso.");
            user.Email = req.Email!;
        }

        if (req.IsActive.HasValue) user.IsActive = req.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = actor;

        await db.SaveChangesAsync(ct);
        return Map(user, ec);
    }

    public async Task DeleteAsync(int id, int ownerUserId, CancellationToken ct)
    {
        var company = await db.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId, ct)
            ?? throw new InvalidOperationException("Empresa no encontrada.");

        var ec = await db.EmployeeCompanies
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == id && x.CompanyId == company.Id, ct)
            ?? throw new KeyNotFoundException("Empleado no encontrado.");

        db.Users.Remove(ec.User);
        db.EmployeeCompanies.Remove(ec);

        await db.SaveChangesAsync(ct);
    }
}
