namespace LoginAndCrud.Domain
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string? UpdatedBy { get; set; }

        public ICollection<ActivityCategory> ActivityCategories { get; set; } = new List<ActivityCategory>();

    }
}
