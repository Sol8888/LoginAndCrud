namespace LoginAndCrud.Contracts
{
    public record AddReviewRequest(
        int ActivityId,
        byte Rating,
        string? Title,
        string? Comment
    );

    public record ReviewResponse(
        long Id,
        int UserId,
        int ActivityId,
        byte Rating,
        string? Title,
        string? Comment,
        DateTime CreatedAt
    );

    public record ActivityReviewsResponse(
        int ActivityId,
        double AvgRating,
        int RatingCount,
        List<ReviewResponse> Reviews
    );


}
