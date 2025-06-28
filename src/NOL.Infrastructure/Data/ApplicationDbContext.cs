using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Branch> Branches { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingExtra> BookingExtras { get; set; }
    public DbSet<ExtraTypePrice> ExtraTypePrices { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SystemSettings> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ApplicationUser relationships
        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Favorites)
            .WithOne(f => f.User)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Reviews)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Car relationships
        modelBuilder.Entity<Car>()
            .HasOne(c => c.Category)
            .WithMany(cat => cat.Cars)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Car>()
            .HasOne(c => c.Branch)
            .WithMany(b => b.Cars)
            .HasForeignKey(c => c.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Booking relationships
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Car)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CarId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure BookingExtra relationships
        modelBuilder.Entity<BookingExtra>()
            .HasOne(be => be.Booking)
            .WithMany(b => b.BookingExtras)
            .HasForeignKey(be => be.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookingExtra>()
            .HasOne(be => be.ExtraTypePrice)
            .WithMany(etp => etp.BookingExtras)
            .HasForeignKey(be => be.ExtraTypePriceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Payment relationships
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Favorite relationships
        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Car)
            .WithMany(c => c.Favorites)
            .HasForeignKey(f => f.CarId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Review relationships
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Car)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CarId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure enum conversions
        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.PreferredLanguage)
            .HasConversion<int>();

        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.UserRole)
            .HasConversion<int>();

        modelBuilder.Entity<Car>()
            .Property(c => c.TransmissionType)
            .HasConversion<int>();

        modelBuilder.Entity<Car>()
            .Property(c => c.FuelType)
            .HasConversion<int>();

        modelBuilder.Entity<Car>()
            .Property(c => c.Status)
            .HasConversion<int>();

        modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion<int>();

        modelBuilder.Entity<Payment>()
            .Property(p => p.PaymentMethod)
            .HasConversion<int>();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<int>();

        modelBuilder.Entity<ExtraTypePrice>()
            .Property(etp => etp.ExtraType)
            .HasConversion<int>();

        modelBuilder.Entity<Notification>()
            .Property(n => n.Type)
            .HasConversion<int>();

        modelBuilder.Entity<SystemSettings>()
            .Property(s => s.Type)
            .HasConversion<int>();

        // Configure decimal precision
        modelBuilder.Entity<Car>()
            .Property(c => c.DailyRate)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Car>()
            .Property(c => c.WeeklyRate)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Car>()
            .Property(c => c.MonthlyRate)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalDays)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.CarRentalCost)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.ExtrasCost)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalCost)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.DiscountAmount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.FinalAmount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<ExtraTypePrice>()
            .Property(etp => etp.DailyPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<ExtraTypePrice>()
            .Property(etp => etp.WeeklyPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<ExtraTypePrice>()
            .Property(etp => etp.MonthlyPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<BookingExtra>()
            .Property(be => be.UnitPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<BookingExtra>()
            .Property(be => be.TotalPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Branch>()
            .Property(b => b.Latitude)
            .HasPrecision(10, 8);

        modelBuilder.Entity<Branch>()
            .Property(b => b.Longitude)
            .HasPrecision(11, 8);

        // Configure unique constraints
        modelBuilder.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.CarId })
            .IsUnique();

        modelBuilder.Entity<Car>()
            .HasIndex(c => c.PlateNumber)
            .IsUnique();

        modelBuilder.Entity<SystemSettings>()
            .HasIndex(s => s.Key)
            .IsUnique();

        // Configure string lengths
        modelBuilder.Entity<Car>()
            .Property(c => c.PlateNumber)
            .HasMaxLength(20);

        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.FirstName)
            .HasMaxLength(100);

        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.LastName)
            .HasMaxLength(100);

        modelBuilder.Entity<Booking>()
            .Property(b => b.BookingNumber)
            .HasMaxLength(50);

        modelBuilder.Entity<Payment>()
            .Property(p => p.PaymentReference)
            .HasMaxLength(100);
    }
} 