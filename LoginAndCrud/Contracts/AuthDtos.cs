namespace LoginAndCrud.Contracts;

public record RegisterRequest(string Username, string Email, string Password, string? CreatedBy);
public record LoginRequest(string UsernameOrEmail, string Password);
public record AuthResponse(int Id, string Username, string Email, string Role, string Token);
