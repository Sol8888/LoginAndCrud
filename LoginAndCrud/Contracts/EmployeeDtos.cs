namespace LoginAndCrud.Contracts;

public record EmployeeResponse(
    int Id,
    string Username,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy,
    string? RoleInCompany,
    int CompanyId
);

public record CreateEmployeeRequest(
    string Username,
    string Email,
    string Password,
    string? RoleInCompany
);

public record PagedEmployeesResponse(
    int Page,
    int PageSize,
    int Total,
    IEnumerable<EmployeeResponse> Items
);