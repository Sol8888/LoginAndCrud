namespace LoginAndCrud.Contracts;

public class CreatePaymentRequestDtos
{
    public decimal Amount { get; set; }
    public int ReservationId { get; set; }
}
