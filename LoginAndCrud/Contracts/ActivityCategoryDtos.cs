namespace LoginAndCrud.Contracts;

public record AddCategoryToActivityRequest(int CategoryId);

public record ActivityWithCategoriesResponse
{
    public int ActivityId { get; init; }
    public string Title { get; init; } = string.Empty;
    public List<CategoryDto> Categories { get; init; } = [];
}

public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
