namespace LoginAndCrud.Domain;

public class Activity
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? LocationText { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int? Capacity { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? Status { get; set; }
    public bool? AllowWaitlist { get; set; }
    public decimal? AvgRating { get; set; }
    public int? RatingCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public Company Company { get; set; } = default!;

    public ICollection<ActivityCategory> ActivityCategories { get; set; } = new List<ActivityCategory>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();


}
