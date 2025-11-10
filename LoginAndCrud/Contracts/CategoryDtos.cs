namespace LoginAndCrud.Contracts;

public record CreateCategoryRequest(string Name);

public record UpdateCategoryRequest(string Name);

public record CategoryResponse(
    int Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy
);
