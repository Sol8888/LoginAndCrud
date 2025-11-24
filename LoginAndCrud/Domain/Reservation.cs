using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LoginAndCrud.Domain;

public class Reservation
{
    [Key]
    public long Id { get; set; }

    public int ActivityId { get; set; }
    public Activity Activity { get; set; } = default!;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal TotalAmount => Quantity * UnitPrice;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public long? PaymentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }
}
