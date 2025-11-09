using LoginAndCrud.Domain;
using Microsoft.EntityFrameworkCore;


namespace LoginAndCrud.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies { get; set; }

    public DbSet<EmployeeCompany> EmployeeCompanies => Set<EmployeeCompany>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("Users", "dbo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasMaxLength(50).HasDefaultValue("User");
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<EmployeeCompany>(e =>
        {
            e.ToTable("EmployeeCompany", "dbo");

            // Clave compuesta
            e.HasKey(ec => new { ec.CompanyId, ec.UserId });

            // Relaciones
            e.HasOne(ec => ec.Company)
                .WithMany()
                .HasForeignKey(ec => ec.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ec => ec.User)
                .WithMany()
                .HasForeignKey(ec => ec.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Propiedades opcionales si quieres configurar más detalles
            e.Property(ec => ec.RoleInCompany).HasMaxLength(50);
            e.Property(ec => ec.CreatedBy).HasMaxLength(100);
            e.Property(ec => ec.UpdatedBy).HasMaxLength(100);
        });
    }
}