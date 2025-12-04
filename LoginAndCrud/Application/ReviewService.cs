using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LoginAndCrud.Application
{
    public interface IReviewService
    {
        Task<ReviewResponse> AddAsync(AddReviewRequest req, int userId, string actor, CancellationToken ct);
       Task<ActivityReviewsResponse> GetByActivityAsync(int activityId, CancellationToken ct);
        Task DeleteAsync(int id, int userId, CancellationToken ct);
    }
    public class ReviewService(AppDbContext db) : IReviewService
    {
        private static ReviewResponse Map(Review r) =>
            new(r.Id, r.UserId, r.TargetId, r.Rating, r.Title, r.Comment, r.CreatedAt);

        public async Task<ReviewResponse> AddAsync(AddReviewRequest req, int userId, string actor, CancellationToken ct)
        {
            var hasReservation = await db.Reservations
                .AnyAsync(r => r.UserId == userId && r.ActivityId == req.ActivityId && r.Status == "Confirmed", ct);

            if (!hasReservation)
                throw new InvalidOperationException("Solo puedes calificar actividades que hayas reservado y pagado.");

            var alreadyReviewed = await db.Reviews
               .AnyAsync(r => r.UserId == userId && r.TargetType == TargetType.Activity && r.TargetId == req.ActivityId, ct);

            if (alreadyReviewed)
                throw new InvalidOperationException("Ya calificaste esta actividad.");

            var review = new Review
            {
                UserId = userId,
                TargetType = TargetType.Activity,
                TargetId = req.ActivityId,
                Rating = req.Rating,
                Title = req.Title,
                Comment = req.Comment,
                CreatedAt = DateTime.UtcNow
            };

            db.Reviews.Add(review);

            
            var activity = await db.Activities.FirstOrDefaultAsync(a => a.Id == req.ActivityId, ct)
                ?? throw new KeyNotFoundException("Actividad no encontrada.");

            activity.RatingCount += 1;
            activity.AvgRating = ((activity.AvgRating * (activity.RatingCount - 1)) + req.Rating) / activity.RatingCount;

            await db.SaveChangesAsync(ct);

            return Map(review);
        }

      public async Task<ActivityReviewsResponse> GetByActivityAsync(int activityId, CancellationToken ct)
        {
            var activity = await db.Activities.FirstOrDefaultAsync(a => a.Id == activityId, ct)
                ?? throw new KeyNotFoundException("Actividad no encontrada.");

            var reviews = await db.Reviews
                .Where(r => r.TargetType == TargetType.Activity && r.TargetId == activityId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);

            var reviewResponses = reviews.Select(Map).ToList();

            return new ActivityReviewsResponse(
                activity.Id,
                (double)activity.AvgRating,
                activity.RatingCount ?? 0,reviewResponses
);
        }

        public async Task DeleteAsync(int id, int userId, CancellationToken ct)
        {
            var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, ct)
                ?? throw new KeyNotFoundException("Reseña no encontrada.");

            db.Reviews.Remove(review);

            // Ajustar promedio en actividad
            var activity = await db.Activities.FirstOrDefaultAsync(a => a.Id == review.TargetId, ct);
            if (activity != null && activity.RatingCount > 0)
            {
                activity.RatingCount -= 1;
                if (activity.RatingCount == 0)
                    activity.AvgRating = 0;
                else
                    activity.AvgRating = (decimal)(await db.Reviews
                        .Where(r => r.TargetType == TargetType.Activity && r.TargetId == activity.Id)
                        .AverageAsync(r => r.Rating, ct));
            }

            await db.SaveChangesAsync(ct);
        }
    }

}

