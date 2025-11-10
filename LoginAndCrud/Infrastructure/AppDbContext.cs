using LoginAndCrud.Domain;
using Microsoft.EntityFrameworkCore;


namespace LoginAndCrud.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies { get; set; }

    public DbSet<EmployeeCompany> EmployeeCompanies => Set<EmployeeCompany>();

    public DbSet<Activity> Activities { get; set; } = default!;
    public DbSet<EmployeeCompany> EmployeeCompany { get; set; } = default!;
    //public DbSet<ActivityCategory> ActivityCategories { get; set; } = default!;
    //public DbSet<Category> Categories { get; set; } = default!;


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

            e.Property(ec => ec.RoleInCompany).HasMaxLength(50);
            e.Property(ec => ec.CreatedBy).HasMaxLength(100);
            e.Property(ec => ec.UpdatedBy).HasMaxLength(100);
        });

        b.Entity<Activity>(e =>
        {
            e.ToTable("Activities", "dbo");
            e.HasKey(a => a.Id);
            e.Property(a => a.Title).HasMaxLength(150).IsRequired();
            e.Property(a => a.Description).HasMaxLength(2000);
            e.Property(a => a.LocationText).HasMaxLength(300);
            e.Property(a => a.Currency).HasMaxLength(3);
            e.Property(a => a.Status).HasMaxLength(20);
            e.Property(a => a.CreatedBy).HasMaxLength(100);
            e.Property(a => a.UpdatedBy).HasMaxLength(100);
            e.HasOne(a => a.Company)
             .WithMany(c => c.Activities)
             .HasForeignKey(a => a.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}