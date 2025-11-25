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
using Microsoft.AspNetCore.Authorization;


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
            return BadRequest("Missing Stripe-Signature header.");

        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                _webhookSecret
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook signature validation failed.");
            return BadRequest();
        }

        // Solo manejamos pagos exitosos
        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

            if (paymentIntent?.Metadata == null ||
                !paymentIntent.Metadata.TryGetValue("reservationId", out var reservationIdStr) ||
                !long.TryParse(reservationIdStr, out var reservationId))
            {
                _logger.LogWarning("El webhook no contenía metadata válida con reservationId.");
                return BadRequest("Metadata faltante o inválida.");
            }

            // Verificar si ya existe un Payment con el mismo ProviderTxnId
            var exists = await _context.Payments.AnyAsync(p => p.ProviderTxnId == paymentIntent.Id);
            if (exists)
            {
                _logger.LogInformation("El pago ya fue procesado previamente.");
                return Ok(); // evitar duplicados
            }

            var payment = new Payment
            {
                Provider = "stripe",
                ProviderTxnId = paymentIntent.Id,
                ReservationId = reservationId,
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency?.ToUpper(),
                Status = "succeeded",
                PaidAt = DateTime.UtcNow,
                RawPayload = json
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(); // 👈 ahora payment.Id está disponible

            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation is not null)
            {
                reservation.PaymentId = payment.Id;
                reservation.Status = "Paid";

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Reserva {reservation.Id} actualizada a estado 'Paid'");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar la reserva pagada.");
                }
            }
            else
            {
                _logger.LogWarning($"No se encontró reserva con ID: {reservationId}");
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

    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutRequest req)
    {
        try
        {
            var sessionUrl = await _stripeService.CreateCheckoutSession(req.ReservationId);
            return Ok(new { url = sessionUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la sesión de Stripe Checkout.");
            return StatusCode(500, "Error al crear sesión de pago.");
        }
    }

    [HttpGet("thank-you")]
    public IActionResult ThankYou()
    {
        return Ok("¡Gracias por tu pago! Puedes cerrar esta ventana.");
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> StartCheckout([FromBody] long reservationId)
    {
        try
        {
            var url = await _stripeService.CreateCheckoutSession(reservationId);
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar el checkout de Stripe.");
            return BadRequest("No se pudo iniciar el pago.");
        }
    }



}

