using LoginAndCrud.Application;
using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace LoginAndCrud.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly string _webhookSecret;
    private readonly ILogger<PaymentsController> _logger;
    private readonly IStripeService _stripeService;

    public PaymentsController(AppDbContext context, IConfiguration config, ILogger<PaymentsController> logger, IStripeService stripeService)
    {
        _context = context;
        _webhookSecret = config["Stripe:WebhookSecret"];
        _logger = logger;
        _stripeService = stripeService;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
{
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(stripeSignature))
        {
            return BadRequest("Missing Stripe-Signature header.");
        }

        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _webhookSecret
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    if (paymentIntent.Metadata.TryGetValue("reservationId", out var reservationIdStr) &&
                        long.TryParse(reservationIdStr, out var reservationId))
                    {
                        var payment = new Payment
                        {
                            Provider = "stripe",
                            ProviderTxnId = paymentIntent.Id,
                            ReservationId = reservationId,
                            Amount = paymentIntent.Amount / 100m,
                            Currency = paymentIntent.Currency.ToUpper(),
                            Status = "succeeded",
                            PaidAt = DateTime.UtcNow,
                            RawPayload = json
                        };

                        _context.Payments.Add(payment);

                    
                        var reservation = await _context.Reservations.FindAsync(reservationId);
                        if (reservation is not null)
                        {
                            reservation.PaymentId = payment.Id;
                            reservation.Status = "Paid";
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error al guardar la reserva pagada.");
                            }

                        }

                    }
                    else
                    {
                        _logger.LogWarning("No se encontró metadata 'reservationId' o no es válido.");
                    }
                }
            }

            return Ok();
    }

    [HttpPost("create-intent")]
    public async Task<IActionResult> CreateIntent([FromBody] CreatePaymentRequestDtos req)
    {
        var intent = await _stripeService.CreatePaymentIntent(req.Amount, req.ReservationId);
        return Ok(new
        {
            clientSecret = intent.ClientSecret
        });
    }

}

