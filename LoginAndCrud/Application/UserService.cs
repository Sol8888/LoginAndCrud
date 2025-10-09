using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using LoginAndCrud.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace LoginAndCrud.Application;

public interface IUserService
{
    Task<PagedUsersResponse> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct);
    Task<UserResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<UserResponse> CreateAsync(CreateUserRequest req, string actor, CancellationToken ct);
    Task<UserResponse> UpdateAsync(int id, UpdateUserRequest req, string actor, CancellationToken ct);
    Task ChangePasswordAsync(int id, string currentPassword, string newPassword, string actor, bool bypassCurrent, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct); // hard delete (ver nota abajo)
}
public class UserService(AppDbContext db) : IUserService
{
    private static UserResponse Map(User u) =>
        new(u.Id, u.Username, u.Email, u.Role, u.IsActive, u.CreatedAt, u.UpdatedAt, u.CreatedBy, u.UpdatedBy);

    public async Task<PagedUsersResponse> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 200 ? 20 : pageSize;

        var query = db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => Map(u))
            .ToListAsync(ct);

        return new(page, pageSize, total, items);
    }

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var u = await db.Users.FindAsync([id], ct);
        return u is null ? null : Map(u);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest req, string actor, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(x => x.Username == req.Username || x.Email == req.Email, ct))
            throw new InvalidOperationException("Username o Email ya existen.");

        var (hash, salt) = PasswordHasher.Hash(req.Password);
        var u = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = string.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor
        };
        db.Users.Add(u);
        await db.SaveChangesAsync(ct);
        return Map(u);
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest req, string actor, CancellationToken ct)
    {
        var u = await db.Users.FindAsync([id], ct) ?? throw new KeyNotFoundException("Usuario no existe.");

        if (!string.IsNullOrWhiteSpace(req.Email) && !string.Equals(u.Email, req.Email, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await db.Users.AnyAsync(x => x.Email == req.Email && x.Id != id, ct);
            if (exists) throw new InvalidOperationException("Email ya en uso.");
            u.Email = req.Email!;
        }

        if (!string.IsNullOrWhiteSpace(req.Role)) u.Role = req.Role!;
        if (req.IsActive.HasValue) u.IsActive = req.IsActive.Value;

        u.UpdatedAt = DateTime.UtcNow;
        u.UpdatedBy = actor;

        await db.SaveChangesAsync(ct);
        return Map(u);
    }

    public async Task ChangePasswordAsync(int id, string currentPassword, string newPassword, string actor, bool bypassCurrent, CancellationToken ct)
    {
        var u = await db.Users.FindAsync([id], ct) ?? throw new KeyNotFoundException("Usuario no existe.");

        if (!bypassCurrent)
        {
            var ok = PasswordHasher.Verify(currentPassword, u.PasswordHash, u.PasswordSalt);
            if (!ok) throw new InvalidOperationException("Contraseña actual incorrecta.");
        }

        var (hash, salt) = PasswordHasher.Hash(newPassword);
        u.PasswordHash = hash;
        u.PasswordSalt = salt;
        u.UpdatedAt = DateTime.UtcNow;
        u.UpdatedBy = actor;

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var u = await db.Users.FindAsync([id], ct) ?? throw new KeyNotFoundException("Usuario no existe.");
        db.Users.Remove(u);               // HARD DELETE
        // Alternativa “soft delete”: u.IsActive = false; u.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct);
        await db.SaveChangesAsync(ct);
    }

}
