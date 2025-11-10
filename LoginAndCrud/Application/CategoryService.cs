using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using LoginAndCrud.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;


namespace LoginAndCrud.Application;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync(CancellationToken ct);
    Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest req, string actor, CancellationToken ct);
    Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest req, string actor, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}

public class CategoryService(AppDbContext db) : ICategoryService
{
    private static CategoryResponse Map(Category c) =>
        new(c.Id, c.Name, c.CreatedAt, c.UpdatedAt, c.CreatedBy, c.UpdatedBy);

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync(CancellationToken ct)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return categories.Select(Map);
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var category = await db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return category is null ? null : Map(category);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest req, string actor, CancellationToken ct)
    {
        if (await db.Categories.AnyAsync(c => c.Name == req.Name, ct))
            throw new InvalidOperationException("Ya existe una categoría con este nombre.");

        var category = new Category
        {
            Name = req.Name,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actor
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return Map(category);
    }

    public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest req, string actor, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException("Categoría no encontrada.");

        if (!string.Equals(category.Name, req.Name, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await db.Categories.AnyAsync(c => c.Name == req.Name && c.Id != id, ct);
            if (exists) throw new InvalidOperationException("Ya existe otra categoría con este nombre.");
            category.Name = req.Name;
        }

        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = actor;

        await db.SaveChangesAsync(ct);
        return Map(category);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException("Categoría no encontrada.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
    }
}
