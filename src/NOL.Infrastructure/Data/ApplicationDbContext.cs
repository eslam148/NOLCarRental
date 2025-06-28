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
    public DbSet<Advertisement> Advertisements { get; set; }
    public DbSet<LoyaltyPointTransaction> LoyaltyPointTransactions { get; set; }

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

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.ReceivingBranch)
            .WithMany()
            .HasForeignKey(b => b.ReceivingBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.DeliveryBranch)
            .WithMany()
            .HasForeignKey(b => b.DeliveryBranchId)
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

        // Configure Advertisement relationships
        modelBuilder.Entity<Advertisement>()
            .HasOne(a => a.Car)
            .WithMany(c => c.Advertisements)
            .HasForeignKey(a => a.CarId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Advertisement>()
            .HasOne(a => a.Category)
            .WithMany(c => c.Advertisements)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Advertisement>()
            .HasOne(a => a.CreatedByUser)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

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

        modelBuilder.Entity<Advertisement>()
            .Property(a => a.Type)
            .HasConversion<int>();

        modelBuilder.Entity<Advertisement>()
            .Property(a => a.Status)
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
            .HasPrecision(10, 2);

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
            .HasPrecision(9, 6);

        modelBuilder.Entity<Branch>()
            .Property(b => b.Longitude)
            .HasPrecision(9, 6);

        // Configure Advertisement decimal precision
        modelBuilder.Entity<Advertisement>()
            .Property(a => a.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Advertisement>()
            .Property(a => a.DiscountPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Advertisement>()
            .Property(a => a.DiscountPercentage)
            .HasPrecision(5, 2);

        // Configure string max lengths
        modelBuilder.Entity<Car>()
            .Property(c => c.PlateNumber)
            .HasMaxLength(20);

        modelBuilder.Entity<SystemSettings>()
            .Property(s => s.Key)
            .HasMaxLength(100);

        modelBuilder.Entity<SystemSettings>()
            .Property(s => s.Value)
            .HasMaxLength(1000);

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
        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.FullName)
            .HasMaxLength(100);

       

        modelBuilder.Entity<Booking>()
            .Property(b => b.BookingNumber)
            .HasMaxLength(50);

        modelBuilder.Entity<Payment>()
            .Property(p => p.PaymentReference)
            .HasMaxLength(100);

        // Configure SystemSettings
        modelBuilder.Entity<SystemSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.Value)
                .HasMaxLength(1000)
                .IsRequired();
            entity.Property(e => e.Type)
                .HasConversion<int>();
        });

        // Configure Branch
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DescriptionAr).HasMaxLength(1000);
            entity.Property(e => e.DescriptionEn).HasMaxLength(1000);
            entity.Property(e => e.Address).HasMaxLength(500).IsRequired();
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Country).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Latitude).HasPrecision(9, 6);
            entity.Property(e => e.Longitude).HasPrecision(9, 6);
            entity.Property(e => e.WorkingHours).HasMaxLength(500);
        });

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).HasMaxLength(100).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DescriptionAr).HasMaxLength(500);
            entity.Property(e => e.DescriptionEn).HasMaxLength(500);
        });

        // Configure Car
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BrandAr).HasMaxLength(100).IsRequired();
            entity.Property(e => e.BrandEn).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ModelAr).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ModelEn).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PlateNumber).HasMaxLength(20);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.DailyRate).HasPrecision(10, 2);
            entity.Property(e => e.WeeklyRate).HasPrecision(10, 2);
            entity.Property(e => e.MonthlyRate).HasPrecision(10, 2);
            entity.Property(e => e.TransmissionType).HasConversion<int>();
            entity.Property(e => e.FuelType).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
        });

        // Configure ExtraTypePrice
        modelBuilder.Entity<ExtraTypePrice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DescriptionAr).HasMaxLength(1000);
            entity.Property(e => e.DescriptionEn).HasMaxLength(1000);
            entity.Property(e => e.DailyPrice).HasPrecision(10, 2);
            entity.Property(e => e.WeeklyPrice).HasPrecision(10, 2);
            entity.Property(e => e.MonthlyPrice).HasPrecision(10, 2);
            entity.Property(e => e.ExtraType).HasConversion<int>();
        });

        // Configure Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookingNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalDays).HasPrecision(10, 2);
            entity.Property(e => e.CarRentalCost).HasPrecision(10, 2);
            entity.Property(e => e.ExtrasCost).HasPrecision(10, 2);
            entity.Property(e => e.TotalCost).HasPrecision(10, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
            entity.Property(e => e.FinalAmount).HasPrecision(10, 2);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
        });

        // Configure BookingExtra
        modelBuilder.Entity<BookingExtra>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
        });

        // Configure Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.PaymentMethod).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.PaymentGatewayResponse).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(500);
        });

        // Configure Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(1000);
        });

        // Configure Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TitleAr).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleEn).HasMaxLength(200).IsRequired();
            entity.Property(e => e.MessageAr).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.MessageEn).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Type).HasConversion<int>();
        });

        // Configure Advertisement
        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TitleAr).IsRequired();
            entity.Property(e => e.TitleEn).IsRequired();
            entity.Property(e => e.DescriptionAr).IsRequired();
            entity.Property(e => e.DescriptionEn).IsRequired();
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.DiscountPrice).HasPrecision(10, 2);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
        });

        // Configure LoyaltyPointTransaction
        modelBuilder.Entity<LoyaltyPointTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Points).IsRequired();
            entity.Property(e => e.TransactionType).HasConversion<int>().IsRequired();
            entity.Property(e => e.EarnReason).HasConversion<int>();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure LoyaltyPointTransaction relationships
        modelBuilder.Entity<LoyaltyPointTransaction>()
            .HasOne(lpt => lpt.User)
            .WithMany(u => u.LoyaltyPointTransactions)
            .HasForeignKey(lpt => lpt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LoyaltyPointTransaction>()
            .HasOne(lpt => lpt.Booking)
            .WithMany()
            .HasForeignKey(lpt => lpt.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 