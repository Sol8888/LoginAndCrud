using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;

namespace LoginAndCrud.Application
{
    public class CategoryFactory
    {
        public static Category Create(CreateCategoryRequest req, string actor)
        {
            return new Category
            {
                Name = req.Name,
                CreatedBy = actor,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void Update(Category category, UpdateCategoryRequest req, string actor)
        {
            category.Name = req.Name;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = actor;
        }
    }
}
