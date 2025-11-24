namespace LoginAndCrud.Contracts;

public record CreateReservationRequest(int ActivityId, int Quantity);

public record ReservationResponse(
    long Id, int ActivityId, int UserId, int Quantity, decimal UnitPrice, decimal TotalAmount,
    string Status, DateTime ReservedAt, DateTime? ExpiresAt, string? CreatedBy
);

public record PagedReservationsResponse(
    int Page, int PageSize, int Total, IEnumerable<ReservationResponse> Items
);