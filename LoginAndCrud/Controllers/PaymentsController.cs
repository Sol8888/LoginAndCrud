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

   
}
