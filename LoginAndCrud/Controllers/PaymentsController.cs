using LoginAndCrud.Application;
using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace LoginAndCrud.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly StripeService _stripe;

    public PaymentsController(StripeService stripe)
    {
        _stripe = stripe;
    }

    [HttpPost("create-stripe-session")]
    public async Task<IActionResult> CreateStripeSession(CreateStripePaymentRequest req)
    {
        var url = await _stripe.CreateCheckoutSession(req.ReservationId);
        return Ok(new { url });
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"];
        var endpointSecret = _stripe.Config["Stripe:WebhookSecret"];

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, endpointSecret);
        }
        catch (Exception)
        {
            return BadRequest("Firma inválida");
        }

        if (stripeEvent.Type == Events.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            if (session?.Metadata.TryGetValue("reservationId", out var resIdStr) == true && long.TryParse(resIdStr, out var reservationId))
            {
                var db = _stripe.Db;

                var reservation = await db.Reservations.FindAsync(reservationId);
                if (reservation == null) return NotFound("Reserva no encontrada");

                // Verificar si ya existe pago para esta reserva
                var exists = await db.Payments.AnyAsync(p => p.ReservationId == reservation.Id && p.ProviderTxnId == session.PaymentIntentId);
                if (!exists)
                {
                    var payment = new Payment
                    {
                        ReservationId = reservation.Id,
                        Provider = "Stripe",
                        ProviderTxnId = session.PaymentIntentId ?? "unknown",
                        Amount = reservation.TotalAmount,
                        Status = "Paid",
                        Currency = "USD",
                        PaidAt = DateTime.UtcNow,
                        RawPayload = json
                    };

                    db.Payments.Add(payment);

                    reservation.Status = "Confirmed";
                    reservation.PaymentId = payment.Id;

                    await db.SaveChangesAsync();
                }
            }
        }

        return Ok();
    }
}
