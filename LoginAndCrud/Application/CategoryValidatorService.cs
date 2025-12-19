using LoginAndCrud.Contracts;
using LoginAndCrud.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LoginAndCrud.Application
{
    public interface ICategoryValidator
    {
        Task ValidateCreateAsync(CreateCategoryRequest request, CancellationToken ct);
        Task ValidateUpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct);
    }
    public class CategoryValidatorService(AppDbContext db) : ICategoryValidator
    {
        public async Task ValidateCreateAsync(CreateCategoryRequest request, CancellationToken ct)
        {
            var exists = await db.Categories.AnyAsync(c => c.Name == request.Name, ct);
            if (exists)
                throw new InvalidOperationException("Ya existe una categoría con este nombre.");
        }

        public async Task ValidateUpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct)
        {
            var exists = await db.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id, ct);
            if (exists)
                throw new InvalidOperationException("Ya existe otra categoría con este nombre.");
        }
    }
}
