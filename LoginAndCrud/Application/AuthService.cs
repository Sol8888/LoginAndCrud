using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using LoginAndCrud.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;



namespace LoginAndCrud.Application;
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct);
}


public class AuthService(AppDbContext db, IJwtTokenFactory jwt) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Username == req.Username || u.Email == req.Email, ct))
            throw new InvalidOperationException("Username o Email ya existen.");

        var (hash, salt) = Infrastructure.Security.PasswordHasher.Hash(req.Password);
        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = req.CreatedBy
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var token = jwt.CreateToken(user.Id, user.Username, user.Role);
        return new AuthResponse(user.Id, user.Username, user.Email, user.Role, token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await db.Users
            .Where(u => u.Username == req.UsernameOrEmail || u.Email == req.UsernameOrEmail)
            .SingleOrDefaultAsync(ct) ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (!user.IsActive) throw new InvalidOperationException("Usuario inactivo.");

        var ok = Infrastructure.Security.PasswordHasher.Verify(req.Password, user.PasswordHash, user.PasswordSalt);
        if (!ok) throw new InvalidOperationException("Credenciales inválidas.");

        var token = jwt.CreateToken(user.Id, user.Username, user.Role);
        return new AuthResponse(user.Id, user.Username, user.Email, user.Role, token);
    }

}
