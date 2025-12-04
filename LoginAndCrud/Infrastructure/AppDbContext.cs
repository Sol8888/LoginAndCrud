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
    public DbSet<ActivityCategory> ActivityCategories { get; set; }
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Reservation> Reservations { get; set; } = default!;
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews { get; set; } = default!;

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
                .WithMany(c => c.Employees)
                .HasForeignKey(ec => ec.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ec => ec.User)
                .WithMany()
                .HasForeignKey(ec => ec.UserId)
                .OnDelete(DeleteBehavior.Restrict);

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

        b.Entity<ActivityCategory>(e =>
        {
            e.ToTable("ActivityCategory", "dbo");

            // Clave compuesta
            e.HasKey(ac => new { ac.ActivityId, ac.CategoryId });

            // Relaciones
            e.HasOne(ac => ac.Activity)
                .WithMany(a => a.ActivityCategories)
                .HasForeignKey(ac => ac.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ac => ac.Category)
                .WithMany(c => c.ActivityCategories)
                .HasForeignKey(ac => ac.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Propiedades adicionales
            e.Property(ac => ac.CreatedBy).HasMaxLength(100);
            e.Property(ac => ac.UpdatedBy).HasMaxLength(100);
        });
        b.Entity<Review>(e =>
        {
            e.ToTable("Reviews", "dbo");
            e.HasKey(r => r.Id);

            e.Property(r => r.TargetType).HasMaxLength(50).IsRequired();
            e.Property(r => r.Title).HasMaxLength(200);
            e.Property(r => r.Comment).HasMaxLength(2000);
            e.Property(r => r.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Activity)
                .WithMany(a => a.Reviews)
                .HasForeignKey(r => r.TargetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RESERVATION
        b.Entity<Reservation>(e =>
        {
            e.ToTable("Reservations", "dbo");
            e.HasKey(r => r.Id);

            e.HasOne(r => r.Activity)
                .WithMany(a => a.Reservations)
                .HasForeignKey(r => r.ActivityId)
                .OnDelete(DeleteBehavior.Restrict); // cascada aquí

            e.HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade); // evita cascada
        });

    }
    }