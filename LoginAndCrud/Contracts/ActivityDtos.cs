namespace LoginAndCrud.Contracts;

public record ActivityResponse(
    int Id,
    int CompanyId,
    string Title,
    string? Description,
    string? LocationText,
    decimal? Latitude,
    decimal? Longitude,
    DateTime? StartAt,
    DateTime? EndAt,
    int? Capacity,
    decimal? Price,
    string? Currency,
    string? Status,
    bool? AllowWaitlist,
    decimal? AvgRating,
    int? RatingCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy
);

public record CreateActivityRequest(
    string Title,
    string? Description,
    string? LocationText,
    decimal? Latitude,
    decimal? Longitude,
    DateTime? StartAt,
    DateTime? EndAt,
    int? Capacity,
    decimal? Price,
    string? Currency,
    string? Status,
    bool? AllowWaitlist
);

public record UpdateActivityRequest(
    string Title,
    string? Description,
    string? LocationText,
    decimal? Latitude,
    decimal? Longitude,
    DateTime? StartAt,
    DateTime? EndAt,
    int? Capacity,
    decimal? Price,
    string? Currency,
    string? Status,
    bool? AllowWaitlist
);

public record PagedActivitiesResponse(int Page, int PageSize, int Total, List<ActivityResponse> Items);
