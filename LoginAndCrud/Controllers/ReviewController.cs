using LoginAndCrud.Application;
using LoginAndCrud.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LoginAndCrud.Controllers
{
    [ApiController]
    [Route("api/activities")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        [HttpPost("{id}/reviews")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ReviewResponse>> AddReview(int id, [FromBody] AddReviewRequest req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized("No se pudo obtener el ID del usuario.");

            var userId = int.Parse(userIdClaim.Value);


            var actor = User.Identity?.Name ?? "system";
            req = req with { ActivityId = id };

            var result = await _reviewService.AddAsync(req, userId, actor, ct);
            return Ok(result);
        }

        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<ActivityReviewsResponse>> GetReviews(int id, CancellationToken ct)
        {
            var result = await _reviewService.GetByActivityAsync(id, ct);
            return Ok(result);
        }

        [HttpDelete("reviews/{reviewId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteReview(int reviewId, CancellationToken ct)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            await _reviewService.DeleteAsync(reviewId, userId, ct);
            return NoContent();
        }
    }
}