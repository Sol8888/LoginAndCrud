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


namespace LoginAndCrud.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly string _webhookSecret;

    public PaymentsController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _webhookSecret = config["Stripe:WebhookSecret"];
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _webhookSecret
            );

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var intent = stripeEvent.Data.Object as PaymentIntent;

                // Aquí haces lo que necesites, por ejemplo guardar en la base de datos
                var payment = new Payment
                {
                    ReservationId = 0, // debes mapear con tu lógica real
                    Provider = "Stripe",
                    ProviderTxnId = intent.Id,
                    Amount = (decimal)intent.AmountReceived / 100,
                    Currency = intent.Currency.ToUpper(),
                    Status = intent.Status,
                    PaidAt = DateTime.UtcNow,
                    RawPayload = json
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest(e.Message);
        }
    }
}
   
