namespace LoginAndCrud.Contracts;

public record CreateStripePaymentRequest(
    long ReservationId,
    decimal Amount,
    string Currency
);
