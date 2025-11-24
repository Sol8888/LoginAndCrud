using Stripe;
using Stripe.Checkout;
using LoginAndCrud.Domain;
using LoginAndCrud.Contracts;
using LoginAndCrud.Infrastructure;
using Microsoft.EntityFrameworkCore;
using LoginAndCrud.Infrastructure.Security;

namespace LoginAndCrud.Application;

public interface IStripeService
{
    Task<string> CreateCheckoutSession(long reservationId);
    Task<PaymentIntent> CreatePaymentIntent(decimal amount, int reservationId);
}

public class StripeService : IStripeService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public IConfiguration Config => _config;
    public AppDbContext Db => _db;
    public StripeService(IConfiguration config)
    {
        _config = config;
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
    }
    public StripeService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
    }

    public async Task<string> CreateCheckoutSession(long reservationId)
    {
        var reservation = await _db.Reservations.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == reservationId)
            ?? throw new Exception("Reserva no encontrada");

        var successUrl = _config["Stripe:SuccessUrl"];
        var cancelUrl = _config["Stripe:CancelUrl"];

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            Mode = "payment",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmountDecimal = reservation.UnitPrice * 100,
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Actividad #{reservation.ActivityId}"
                        }
                    },
                    Quantity = reservation.Quantity
                }
            },
            Metadata = new Dictionary<string, string>
            {
                { "reservationId", reservation.Id.ToString() }
            },
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return session.Url;
    }

    public async Task<PaymentIntent> CreatePaymentIntent(decimal amount, int reservationId)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // convertir a centavos
            Currency = "usd",
            Metadata = new Dictionary<string, string>
            {
                { "reservationId", reservationId.ToString() }
            }
        };

        var service = new PaymentIntentService();
        return await service.CreateAsync(options);
    }
}
