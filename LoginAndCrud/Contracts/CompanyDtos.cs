using System.ComponentModel.DataAnnotations;

namespace LoginAndCrud.Contracts;

public record CompanyResponse(int Id, string Name, string? Description, int OwnerUserId, decimal AvgRating, int RatingCount, bool IsActive, DateTime CreatedAt,  DateTime? UpdatedAt, string? CreatedBy, string? UpdatedBy
);

public record PagedCompaniesResponse(int Page, int PageSize, int Total, IEnumerable<CompanyResponse> Items);

public record CreateCompanyRequest(string Name, string? Description, int OwnerUserId);

public record UpdateCompanyRequest(string? Name, string? Description, int? OwnerUserId, bool? IsActive);

