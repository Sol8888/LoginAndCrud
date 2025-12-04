using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginAndCrud.Domain
{
    public class Review
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required, MaxLength(50)]
        public TargetType TargetType { get; set; }

        [ForeignKey(nameof(Activity))]
        public int TargetId { get; set; }
        public Activity Activity { get; set; }

        [Range(1, 5)]
        public byte Rating { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
