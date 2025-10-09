namespace LoginAndCrud.Contracts;

public record UserResponse(int Id, string Username, string Email, string Role, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt, string? CreatedBy, string? UpdatedBy);

public record CreateUserRequest(string Username, string Email, string Password, string Role = "User", bool IsActive = true);
public record UpdateUserRequest(string? Email, string? Role, bool? IsActive); // admin puede tocar estos
public record ChangePasswordRequest(string CurrentPassword, string NewPassword); // usuario
public record ResetPasswordRequest(string NewPassword); // admin

public record PagedUsersResponse(int Page, int PageSize, int Total, IEnumerable<UserResponse> Items);

