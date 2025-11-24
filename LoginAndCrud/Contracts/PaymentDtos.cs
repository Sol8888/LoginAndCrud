namespace LoginAndCrud.Contracts;

public record CreateStripePaymentRequest(
    long ReservationId,
    decimal Amount,
    string Currency
);

public class CheckoutRequest
{
    public long ReservationId { get; set; }
}
