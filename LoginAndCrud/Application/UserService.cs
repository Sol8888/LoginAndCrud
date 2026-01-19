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
    Task DeleteAsync(int id, CancellationToken ct); 
}
public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly UserValidator _validator;

    public UserService(IUserRepository repo, UserValidator validator)
    {
        _repo = repo;
        _validator = validator;
    }

    private static UserResponse Map(User u) => new(u.Id, u.Username, u.Email, u.Role, u.IsActive, u.CreatedAt, u.UpdatedAt, u.CreatedBy, u.UpdatedBy);

    public async Task<PagedUsersResponse> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        int skip = (page - 1) * pageSize;
        var items = await _repo.GetPagedAsync(skip, pageSize, search, ct);
        var total = await _repo.CountAsync(search, ct);
        return new(page, pageSize, total, items.Select(Map));
    }

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(id, ct);
        return user is null ? null : Map(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest req, string actor, CancellationToken ct)
    {
        await _validator.ValidateCreateAsync(req, ct);
        var (hash, salt) = PasswordHasher.Hash(req.Password);

        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = req.Role,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor
        };

        await _repo.AddAsync(user, ct);
        await _repo.SaveChangesAsync(ct);

        return Map(user);
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest req, string actor, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Usuario no existe.");

        if (!string.IsNullOrWhiteSpace(req.Email) && !string.Equals(user.Email, req.Email, StringComparison.OrdinalIgnoreCase))
        {
            await _validator.ValidateUpdateEmailAsync(id, req.Email!, ct);
            user.Email = req.Email!;
        }

        if (!string.IsNullOrWhiteSpace(req.Role)) user.Role = req.Role!;
        if (req.IsActive.HasValue) user.IsActive = req.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = actor;

        await _repo.SaveChangesAsync(ct);
        return Map(user);
    }

    public async Task ChangePasswordAsync(int id, string currentPassword, string newPassword, string actor, bool bypassCurrent, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Usuario no existe.");

        if (!bypassCurrent && !PasswordHasher.Verify(currentPassword, user.PasswordHash, user.PasswordSalt))
            throw new InvalidOperationException("Contraseña actual incorrecta.");

        var (hash, salt) = PasswordHasher.Hash(newPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = actor;

        await _repo.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Usuario no existe.");
        await _repo.DeleteAsync(user, ct);
        await _repo.SaveChangesAsync(ct);
    }
}
