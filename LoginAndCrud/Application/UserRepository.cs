using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using Microsoft.EntityFrameworkCore;
using LoginAndCrud.Infrastructure.Security;

namespace LoginAndCrud.Application;

public interface IUserRepository
{
    Task<bool> ExistsByUsernameOrEmailAsync(string username, string email, CancellationToken ct);
    Task<bool> EmailInUseByOtherAsync(int id, string email, CancellationToken ct);
    Task<User?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<User>> GetPagedAsync(int skip, int take, string? search, CancellationToken ct);
    Task<int> CountAsync(string? search, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task DeleteAsync(User user, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}


public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsByUsernameOrEmailAsync(string username, string email, CancellationToken ct)
        => _db.Users.AnyAsync(u => u.Username == username || u.Email == email, ct);

    public Task<bool> EmailInUseByOtherAsync(int id, string email, CancellationToken ct)
        => _db.Users.AnyAsync(u => u.Email == email && u.Id != id, ct);

    public Task<User?> GetByIdAsync(int id, CancellationToken ct)
        => _db.Users.FindAsync([id], ct).AsTask();

    public Task<List<User>> GetPagedAsync(int skip, int take, string? search, CancellationToken ct)
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));

        return query.OrderBy(u => u.Id).Skip(skip).Take(take).ToListAsync(ct);
    }

    public Task<int> CountAsync(string? search, CancellationToken ct)
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));
        return query.CountAsync(ct);
    }

    public Task AddAsync(User user, CancellationToken ct)
    {
        _db.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken ct)
    {
        _db.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}

