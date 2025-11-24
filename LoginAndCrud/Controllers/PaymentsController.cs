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

    public PaymentsController(AppDbContext context, IConfiguration config, ILogger<PaymentsController> logger)
    {
        _context = context;
        _webhookSecret = config["Stripe:WebhookSecret"];
        _logger = logger;
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
            var payment = new Payment
            {
                Provider = "stripe",
                ProviderTxnId = paymentIntent.Id,
                ReservationId = 1, // TODO: actualizar con lógica real
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency.ToUpper(),
                Status = "succeeded",
                PaidAt = DateTime.UtcNow,
                RawPayload = json
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }
    }

    return Ok();
}
}
   
