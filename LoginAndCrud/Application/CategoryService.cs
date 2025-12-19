using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using LoginAndCrud.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace LoginAndCrud.Application;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync(CancellationToken ct);
    Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest req, string actor, CancellationToken ct);
    Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest req, string actor, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}

public class CategoryService(AppDbContext db, ICategoryValidator validator) : ICategoryService
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
        await validator.ValidateCreateAsync(req, ct);

        var category = CategoryFactory.Create(req, actor);
        db.Categories.Add(category);

        await db.SaveChangesAsync(ct);
        return Map(category);
    }

    public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest req, string actor, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException("Categoría no encontrada.");

        await validator.ValidateUpdateAsync(id, req, ct);

        CategoryFactory.Update(category, req, actor);
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
