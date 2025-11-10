namespace LoginAndCrud.Domain;

public class ActivityCategory
{
    public int ActivityId { get; set; }
    public int CategoryId { get; set; }

    public Activity? Activity { get; set; }
    public Category? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
