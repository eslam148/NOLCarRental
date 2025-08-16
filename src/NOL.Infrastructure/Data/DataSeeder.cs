using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        try
        {
            // Seed core data first (no foreign key dependencies)
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedSystemSettingsAsync(context);
            await SeedBranchesAsync(context);
            await SeedCategoriesAsync(context);
            await SeedExtraTypePricesAsync(context);
            
            // Save changes for core entities before seeding dependent entities
            await context.SaveChangesAsync();
            
            await SeedCarsAsync(context);
            
            // Save changes for entities that other entities depend on
            await context.SaveChangesAsync();
            
            // Seed entities with foreign key dependencies
            await SeedBookingsAsync(context);
            await SeedPaymentsAsync(context);
            await SeedReviewsAsync(context);
            await SeedNotificationsAsync(context);
            await SeedFavoritesAsync(context);
            await SeedLoyaltyPointsAsync(context);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Re-throw with more context
            throw new Exception($"An error occurred while seeding the database: {ex.Message}", ex);
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Customer", "Employee" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var users = new List<(ApplicationUser user, string password, string role)>();

        // Admin users
        users.Add((new ApplicationUser
        {
            UserName = "admin@nolrental.com",
            Email = "admin@nolrental.com",
            EmailConfirmed = true,
            FullName = "System Administrator",
            
            PhoneNumber = "+971501234567",
            PreferredLanguage = Language.Arabic,
            UserRole = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }, "Admin123!", "Admin"));

        users.Add((new ApplicationUser
        {
            UserName = "manager@nolrental.com",
            Email = "manager@nolrental.com",
            EmailConfirmed = true,
            FullName = " المدير أحمد",
          
            PhoneNumber = "+971502345678",
            PreferredLanguage = Language.Arabic,
            UserRole = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }, "Manager123!", "Admin"));

        // Employee users
        users.Add((new ApplicationUser
        {
            UserName = "employee1@nolrental.com",
            Email = "employee1@nolrental.com",
            EmailConfirmed = true,
            FullName = "سارة موظفة",
        
            PhoneNumber = "+971503456789",
            PreferredLanguage = Language.Arabic,
            UserRole = UserRole.Employee,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }, "Employee123!", "Employee"));

        users.Add((new ApplicationUser
        {
            UserName = "employee2@nolrental.com",
            Email = "employee2@nolrental.com",
            EmailConfirmed = true,
            FullName = "Omar Hassan",
          
            PhoneNumber = "+971504567890",
            PreferredLanguage = Language.English,
            UserRole = UserRole.Employee,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }, "Employee123!", "Employee"));

        // Customer users (16 customers to make ~20 total)
        var customerData = new[]
        {
            ("محمد", "+971509876543", Language.Arabic),
            ("فاطمة", "+971508765432", Language.Arabic),
            ("خالد", "+971507654321", Language.Arabic),
            ("عائشة", "+971506543210", Language.Arabic),
            ("John", "+971505432109", Language.English),
            ("Emily", "+971504321098", Language.English),
            ("David", "+971503210987", Language.English),
            ("Sarah", "+971502109876", Language.English),
            ("عبدالله", "+971501098765", Language.Arabic),
            ("مريم", "+971500987654", Language.Arabic),
            ("أحمد", "+971509987654", Language.Arabic),
            ("نورا", "+971508987654", Language.Arabic),
            ("Michael" , "+971507987654", Language.English),
            ("Jessica", "+971506987654", Language.English),
            ("Daniel", "+971505987654", Language.English),
            ("Lisa", "+971504987654", Language.English)
        };

        for (int i = 0; i < customerData.Length; i++)
        {
            var (firstName, phone, language) = customerData[i];
            var email = $"customer{i + 1}@example.com";
            
            users.Add((new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = firstName,
               
                PhoneNumber = phone,
                PreferredLanguage = language,
                UserRole = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365))
            }, "Customer123!", "Customer"));
        }

        // Create all users
        foreach (var (user, password, role) in users)
        {
            if (await userManager.FindByEmailAsync(user.Email!) == null)
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }

    private static async Task SeedSystemSettingsAsync(ApplicationDbContext context)
    {
        if (!await context.SystemSettings.AnyAsync())
        {
            var settings = new List<SystemSettings>
            {
                new SystemSettings { Key = "CompanyName", Value = "NOL Car Rental", Type = SettingType.String, Description = "Company Name", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "CompanyPhone", Value = "+971-4-123-4567", Type = SettingType.String, Description = "Main Company Phone", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "CompanyEmail", Value = "info@nolrental.com", Type = SettingType.String, Description = "Main Company Email", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "DefaultLanguage", Value = "ar", Type = SettingType.String, Description = "Default System Language", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "BookingCancellationHours", Value = "24", Type = SettingType.Number, Description = "Hours before booking start to allow free cancellation", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "MinBookingHours", Value = "2", Type = SettingType.Number, Description = "Minimum booking duration in hours", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "MaxBookingDays", Value = "30", Type = SettingType.Number, Description = "Maximum booking duration in days", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "SecurityDeposit", Value = "500.00", Type = SettingType.Decimal, Description = "Security deposit amount in AED", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "LateFeePerDay", Value = "50.00", Type = SettingType.Decimal, Description = "Late return fee per day in AED", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "FuelPolicyType", Value = "full-to-full", Type = SettingType.String, Description = "Fuel policy for rentals", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "MinDriverAge", Value = "21", Type = SettingType.Number, Description = "Minimum driver age requirement", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "MaxDriverAge", Value = "75", Type = SettingType.Number, Description = "Maximum driver age limit", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "RequiredLicenseYears", Value = "2", Type = SettingType.Number, Description = "Minimum driving license experience in years", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "WeeklyDiscountPercent", Value = "10", Type = SettingType.Number, Description = "Weekly rental discount percentage", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "MonthlyDiscountPercent", Value = "25", Type = SettingType.Number, Description = "Monthly rental discount percentage", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "VATPercentage", Value = "5", Type = SettingType.Number, Description = "VAT percentage", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "EnableSMSNotifications", Value = "true", Type = SettingType.Boolean, Description = "Enable SMS notifications", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "EnableEmailNotifications", Value = "true", Type = SettingType.Boolean, Description = "Enable email notifications", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "BusinessHoursStart", Value = "08:00", Type = SettingType.String, Description = "Standard business hours start time", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "BusinessHoursEnd", Value = "22:00", Type = SettingType.String, Description = "Standard business hours end time", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SystemSettings { Key = "MaxAdvanceBookingDays", Value = "90", Type = SettingType.Number, Description = "Maximum days in advance for booking", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            context.SystemSettings.AddRange(settings);
        }
    }

    private static async Task SeedBranchesAsync(ApplicationDbContext context)
    {
        if (!await context.Branches.AnyAsync())
        {
            var branches = new List<Branch>
            {
                // UAE Branches
                new Branch { NameAr = "فرع دبي الرئيسي", NameEn = "Dubai Main Branch", DescriptionAr = "الفرع الرئيسي في قلب دبي مع خدمة على مدار الساعة", DescriptionEn = "Main branch in the heart of Dubai with 24/7 service", Address = "Sheikh Zayed Road, Dubai", City = "Dubai", Country = "UAE", Phone = "+971-4-123-4567", Email = "dubai@nolrental.com", Latitude = 25.2048m, Longitude = 55.2708m, WorkingHours = "24/7", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع دبي مول", NameEn = "Dubai Mall Branch", DescriptionAr = "فرع دبي مول في وسط المدينة", DescriptionEn = "Dubai Mall branch in downtown", Address = "Downtown Dubai, Dubai Mall", City = "Dubai", Country = "UAE", Phone = "+971-4-234-5678", Email = "dubaimall@nolrental.com", Latitude = 25.1972m, Longitude = 55.2744m, WorkingHours = "10:00 - 24:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع مطار دبي", NameEn = "Dubai Airport Branch", DescriptionAr = "فرع مطار دبي الدولي", DescriptionEn = "Dubai International Airport branch", Address = "Terminal 1, Dubai International Airport", City = "Dubai", Country = "UAE", Phone = "+971-4-345-6789", Email = "dubaiairport@nolrental.com", Latitude = 25.2532m, Longitude = 55.3657m, WorkingHours = "24/7", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع دبي مارينا", NameEn = "Dubai Marina Branch", DescriptionAr = "فرع دبي مارينا على الواجهة البحرية", DescriptionEn = "Dubai Marina waterfront branch", Address = "Dubai Marina Walk", City = "Dubai", Country = "UAE", Phone = "+971-4-456-7890", Email = "dubaimarina@nolrental.com", Latitude = 25.0657m, Longitude = 55.1377m, WorkingHours = "08:00 - 23:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع أبوظبي الرئيسي", NameEn = "Abu Dhabi Main Branch", DescriptionAr = "فرع أبوظبي في منطقة الكورنيش", DescriptionEn = "Abu Dhabi branch in Corniche area", Address = "Corniche Road, Abu Dhabi", City = "Abu Dhabi", Country = "UAE", Phone = "+971-2-234-5678", Email = "abudhabi@nolrental.com", Latitude = 24.4539m, Longitude = 54.3773m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع مطار أبوظبي", NameEn = "Abu Dhabi Airport Branch", DescriptionAr = "فرع مطار أبوظبي الدولي", DescriptionEn = "Abu Dhabi International Airport branch", Address = "Abu Dhabi International Airport", City = "Abu Dhabi", Country = "UAE", Phone = "+971-2-345-6789", Email = "adairport@nolrental.com", Latitude = 24.4330m, Longitude = 54.6511m, WorkingHours = "24/7", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع الشارقة", NameEn = "Sharjah Branch", DescriptionAr = "فرع الشارقة بالقرب من المطار", DescriptionEn = "Sharjah branch near the airport", Address = "Airport Road, Sharjah", City = "Sharjah", Country = "UAE", Phone = "+971-6-345-6789", Email = "sharjah@nolrental.com", Latitude = 25.3463m, Longitude = 55.4209m, WorkingHours = "06:00 - 24:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع عجمان", NameEn = "Ajman Branch", DescriptionAr = "فرع عجمان في الكورنيش", DescriptionEn = "Ajman Corniche branch", Address = "Ajman Corniche", City = "Ajman", Country = "UAE", Phone = "+971-6-456-7890", Email = "ajman@nolrental.com", Latitude = 25.4052m, Longitude = 55.5136m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع رأس الخيمة", NameEn = "Ras Al Khaimah Branch", DescriptionAr = "فرع رأس الخيمة في المدينة", DescriptionEn = "Ras Al Khaimah city branch", Address = "Al Qawasim Corniche", City = "Ras Al Khaimah", Country = "UAE", Phone = "+971-7-234-5678", Email = "rak@nolrental.com", Latitude = 25.7889m, Longitude = 55.9414m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع الفجيرة", NameEn = "Fujairah Branch", DescriptionAr = "فرع الفجيرة على الساحل الشرقي", DescriptionEn = "Fujairah East Coast branch", Address = "Fujairah Corniche", City = "Fujairah", Country = "UAE", Phone = "+971-9-345-6789", Email = "fujairah@nolrental.com", Latitude = 25.1171m, Longitude = 56.3267m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع أم القيوين", NameEn = "Umm Al Quwain Branch", DescriptionAr = "فرع أم القيوين", DescriptionEn = "Umm Al Quwain branch", Address = "King Faisal Road", City = "Umm Al Quwain", Country = "UAE", Phone = "+971-6-567-8901", Email = "uaq@nolrental.com", Latitude = 25.5641m, Longitude = 55.6550m, WorkingHours = "08:00 - 20:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع العين", NameEn = "Al Ain Branch", DescriptionAr = "فرع العين مدينة الحدائق", DescriptionEn = "Al Ain Garden City branch", Address = "Al Ain Mall Area", City = "Al Ain", Country = "UAE", Phone = "+971-3-456-7890", Email = "alain@nolrental.com", Latitude = 24.2075m, Longitude = 55.7447m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                
                // International Branches
                new Branch { NameAr = "فرع الرياض", NameEn = "Riyadh Branch", DescriptionAr = "فرع الرياض في المملكة العربية السعودية", DescriptionEn = "Riyadh branch in Saudi Arabia", Address = "King Fahd Road", City = "Riyadh", Country = "Saudi Arabia", Phone = "+966-11-234-5678", Email = "riyadh@nolrental.com", Latitude = 24.7136m, Longitude = 46.6753m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع جدة", NameEn = "Jeddah Branch", DescriptionAr = "فرع جدة على ساحل البحر الأحمر", DescriptionEn = "Jeddah Red Sea branch", Address = "Corniche Road", City = "Jeddah", Country = "Saudi Arabia", Phone = "+966-12-345-6789", Email = "jeddah@nolrental.com", Latitude = 21.4858m, Longitude = 39.1925m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع الدوحة", NameEn = "Doha Branch", DescriptionAr = "فرع الدوحة في قطر", DescriptionEn = "Doha branch in Qatar", Address = "Doha Corniche", City = "Doha", Country = "Qatar", Phone = "+974-4-456-7890", Email = "doha@nolrental.com", Latitude = 25.2854m, Longitude = 51.5310m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع مسقط", NameEn = "Muscat Branch", DescriptionAr = "فرع مسقط في سلطنة عمان", DescriptionEn = "Muscat branch in Oman", Address = "Sultan Qaboos Street", City = "Muscat", Country = "Oman", Phone = "+968-2-567-8901", Email = "muscat@nolrental.com", Latitude = 23.5859m, Longitude = 58.4059m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع المنامة", NameEn = "Manama Branch", DescriptionAr = "فرع المنامة في البحرين", DescriptionEn = "Manama branch in Bahrain", Address = "Government Avenue", City = "Manama", Country = "Bahrain", Phone = "+973-1-678-9012", Email = "manama@nolrental.com", Latitude = 26.2285m, Longitude = 50.5860m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع الكويت", NameEn = "Kuwait City Branch", DescriptionAr = "فرع الكويت في دولة الكويت", DescriptionEn = "Kuwait City branch in Kuwait", Address = "Arabian Gulf Street", City = "Kuwait City", Country = "Kuwait", Phone = "+965-2-789-0123", Email = "kuwait@nolrental.com", Latitude = 29.3117m, Longitude = 47.4818m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع دبي الجنوب", NameEn = "Dubai South Branch", DescriptionAr = "فرع دبي الجنوب بالقرب من إكسبو", DescriptionEn = "Dubai South branch near Expo area", Address = "Dubai South, Expo Road", City = "Dubai", Country = "UAE", Phone = "+971-4-567-8901", Email = "dubaisouth@nolrental.com", Latitude = 24.8925m, Longitude = 55.1611m, WorkingHours = "08:00 - 22:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Branch { NameAr = "فرع جزيرة ياس", NameEn = "Yas Island Branch", DescriptionAr = "فرع جزيرة ياس للترفيه", DescriptionEn = "Yas Island entertainment branch", Address = "Yas Island, Abu Dhabi", City = "Abu Dhabi", Country = "UAE", Phone = "+971-2-678-9012", Email = "yasland@nolrental.com", Latitude = 24.4877m, Longitude = 54.6099m, WorkingHours = "08:00 - 24:00", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            context.Branches.AddRange(branches);
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category
                {
                    NameAr = "اقتصادية",
                    NameEn = "Economy",
                    DescriptionAr = "سيارات اقتصادية مثالية للاستخدام اليومي",
                    DescriptionEn = "Economical cars perfect for daily use",
                    ImageUrl = "/images/categories/economy.jpg",
                    SortOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Category
                {
                    NameAr = "متوسطة",
                    NameEn = "Mid-Size",
                    DescriptionAr = "سيارات متوسطة الحجم مريحة للعائلات",
                    DescriptionEn = "Mid-size cars comfortable for families",
                    ImageUrl = "/images/categories/midsize.jpg",
                    SortOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Category
                {
                    NameAr = "فاخرة",
                    NameEn = "Luxury",
                    DescriptionAr = "سيارات فاخرة للمناسبات الخاصة",
                    DescriptionEn = "Luxury cars for special occasions",
                    ImageUrl = "/images/categories/luxury.jpg",
                    SortOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Category
                {
                    NameAr = "رياضية",
                    NameEn = "Sports",
                    DescriptionAr = "سيارات رياضية عالية الأداء",
                    DescriptionEn = "High-performance sports cars",
                    ImageUrl = "/images/categories/sports.jpg",
                    SortOrder = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Category
                {
                    NameAr = "دفع رباعي",
                    NameEn = "SUV",
                    DescriptionAr = "سيارات دفع رباعي للرحلات والمغامرات",
                    DescriptionEn = "SUVs for trips and adventures",
                    ImageUrl = "/images/categories/suv.jpg",
                    SortOrder = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Categories.AddRange(categories);
        }
    }

    private static async Task SeedExtraTypePricesAsync(ApplicationDbContext context)
    {
        if (!await context.ExtraTypePrices.AnyAsync())
        {
            var extras = new List<ExtraTypePrice>
            {
                // Essential Services
                new ExtraTypePrice { ExtraType = ExtraType.GPS, NameAr = "نظام تحديد المواقع", NameEn = "GPS Navigation System", DescriptionAr = "نظام تحديد المواقع مع الخرائط المحدثة", DescriptionEn = "GPS navigation system with updated maps", DailyPrice = 25.00m, WeeklyPrice = 150.00m, MonthlyPrice = 500.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.ChildSeat, NameAr = "مقعد أطفال", NameEn = "Child Safety Seat", DescriptionAr = "مقعد أمان للأطفال معتمد دولياً", DescriptionEn = "Internationally certified child safety seat", DailyPrice = 15.00m, WeeklyPrice = 90.00m, MonthlyPrice = 300.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.AdditionalDriver, NameAr = "سائق إضافي", NameEn = "Additional Driver", DescriptionAr = "إضافة سائق إضافي مخول للقيادة", DescriptionEn = "Add an additional authorized driver", DailyPrice = 30.00m, WeeklyPrice = 180.00m, MonthlyPrice = 600.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.Insurance, NameAr = "تأمين إضافي", NameEn = "Additional Insurance", DescriptionAr = "تأمين شامل ضد جميع المخاطر", DescriptionEn = "Comprehensive insurance against all risks", DailyPrice = 50.00m, WeeklyPrice = 300.00m, MonthlyPrice = 1000.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.WifiHotspot, NameAr = "واي فاي محمول", NameEn = "Portable WiFi", DescriptionAr = "جهاز واي فاي محمول مع إنترنت عالي السرعة", DescriptionEn = "Portable WiFi device with high-speed internet", DailyPrice = 20.00m, WeeklyPrice = 120.00m, MonthlyPrice = 400.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                
                // Technology & Connectivity
                new ExtraTypePrice { ExtraType = ExtraType.PhoneCharger, NameAr = "شاحن هاتف", NameEn = "Phone Charger", DescriptionAr = "شاحن سيارة متعدد المنافذ لجميع أنواع الهواتف", DescriptionEn = "Multi-port car charger for all phone types", DailyPrice = 5.00m, WeeklyPrice = 30.00m, MonthlyPrice = 100.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.Bluetooth, NameAr = "بلوتوث محمول", NameEn = "Bluetooth Adapter", DescriptionAr = "جهاز بلوتوث للسيارات القديمة", DescriptionEn = "Bluetooth adapter for older cars", DailyPrice = 10.00m, WeeklyPrice = 60.00m, MonthlyPrice = 200.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                
                // Storage & Equipment
                new ExtraTypePrice { ExtraType = ExtraType.RoofRack, NameAr = "حمالة سقف", NameEn = "Roof Rack", DescriptionAr = "حمالة سقف لنقل الأمتعة الإضافية", DescriptionEn = "Roof rack for additional luggage transport", DailyPrice = 35.00m, WeeklyPrice = 210.00m, MonthlyPrice = 700.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.SkiRack, NameAr = "حمالة تزلج", NameEn = "Ski Rack", DescriptionAr = "حمالة خاصة لمعدات التزلج", DescriptionEn = "Specialized rack for ski equipment", DailyPrice = 40.00m, WeeklyPrice = 240.00m, MonthlyPrice = 800.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.BikeRack, NameAr = "حمالة دراجات", NameEn = "Bike Rack", DescriptionAr = "حمالة لنقل الدراجات الهوائية", DescriptionEn = "Rack for transporting bicycles", DailyPrice = 30.00m, WeeklyPrice = 180.00m, MonthlyPrice = 600.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                
                // Premium Services
                new ExtraTypePrice { ExtraType = ExtraType.GPS, NameAr = "نظام ملاحة متقدم", NameEn = "Premium Navigation", DescriptionAr = "نظام ملاحة متقدم مع تنبيهات حركة المرور", DescriptionEn = "Advanced navigation with traffic alerts", DailyPrice = 35.00m, WeeklyPrice = 210.00m, MonthlyPrice = 700.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.ChildSeat, NameAr = "مقعد أطفال رضع", NameEn = "Infant Car Seat", DescriptionAr = "مقعد خاص للأطفال الرضع (0-12 شهر)", DescriptionEn = "Specialized seat for infants (0-12 months)", DailyPrice = 20.00m, WeeklyPrice = 120.00m, MonthlyPrice = 400.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.ChildSeat, NameAr = "مقعد أطفال كبار", NameEn = "Booster Seat", DescriptionAr = "مقعد مرتفع للأطفال الكبار (4-12 سنة)", DescriptionEn = "Booster seat for older children (4-12 years)", DailyPrice = 12.00m, WeeklyPrice = 72.00m, MonthlyPrice = 240.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                
                // Comfort & Convenience
                new ExtraTypePrice { ExtraType = ExtraType.PhoneCharger, NameAr = "شاحن لاسلكي", NameEn = "Wireless Charger", DescriptionAr = "شاحن لاسلكي لوحة القيادة", DescriptionEn = "Dashboard wireless charging pad", DailyPrice = 15.00m, WeeklyPrice = 90.00m, MonthlyPrice = 300.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.WifiHotspot, NameAr = "إنترنت فائق السرعة", NameEn = "High-Speed Internet", DescriptionAr = "إنترنت فائق السرعة 5G", DescriptionEn = "Ultra-fast 5G internet connection", DailyPrice = 35.00m, WeeklyPrice = 210.00m, MonthlyPrice = 700.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                
                // Safety & Emergency
                new ExtraTypePrice { ExtraType = ExtraType.GPS, NameAr = "نظام طوارئ", NameEn = "Emergency Kit", DescriptionAr = "عدة طوارئ شاملة مع الإسعافات الأولية", DescriptionEn = "Comprehensive emergency kit with first aid", DailyPrice = 10.00m, WeeklyPrice = 60.00m, MonthlyPrice = 200.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.GPS, NameAr = "كاميرا قيادة", NameEn = "Dash Camera", DescriptionAr = "كاميرا تسجيل أثناء القيادة", DescriptionEn = "Dashboard recording camera", DailyPrice = 20.00m, WeeklyPrice = 120.00m, MonthlyPrice = 400.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                
                // Luxury Additions
                new ExtraTypePrice { ExtraType = ExtraType.PhoneCharger, NameAr = "وسائد رقبة فاخرة", NameEn = "Luxury Neck Pillows", DescriptionAr = "وسائد رقبة فاخرة للراحة", DescriptionEn = "Premium comfort neck pillows", DailyPrice = 8.00m, WeeklyPrice = 48.00m, MonthlyPrice = 160.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ExtraTypePrice { ExtraType = ExtraType.Bluetooth, NameAr = "نظام صوت محيطي", NameEn = "Surround Sound System", DescriptionAr = "نظام صوت محيطي فائق الجودة", DescriptionEn = "Premium surround sound audio system", DailyPrice = 25.00m, WeeklyPrice = 150.00m, MonthlyPrice = 500.00m, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            context.ExtraTypePrices.AddRange(extras);
        }
    }

    private static async Task SeedCarsAsync(ApplicationDbContext context)
    {
        if (!await context.Cars.AnyAsync())
        {
            // Get branches and categories for foreign keys
            var branches = await context.Branches.ToListAsync();
            var categories = await context.Categories.ToListAsync();

            var cars = new List<Car>();
            var plateCounter = 10000;
            var random = new Random();

            // Helper method to create cars with variations
            void AddCarsForModel(string brandAr, string brandEn, string modelAr, string modelEn,
                decimal basePrice, string categoryName, int count = 3)
            {
                var category = categories.First(c => c.NameEn == categoryName);
                var colorsAr = new[] { "أبيض", "أسود", "فضي", "أزرق", "أحمر", "رمادي", "أبيض لؤلؤي", "رمادي معدني" };
                var colorsEn = new[] { "White", "Black", "Silver", "Blue", "Red", "Gray", "Pearl White", "Metallic Gray" };

                for (int i = 0; i < count; i++)
                {
                    var colorIndex = random.Next(colorsAr.Length);
                    var seatingCapacity = categoryName == "Sports" ? random.Next(2, 5) : (categoryName == "SUV" ? random.Next(7, 9) : 5);
                    var numberOfDoors = seatingCapacity <= 2 ? 2 : (seatingCapacity <= 5 ? 4 : 5);
                    var maxSpeed = categoryName == "Sports" ? random.Next(250, 350) : (categoryName == "SUV" ? random.Next(180, 220) : random.Next(160, 200));
                    var engine = GetEngineSpecification(categoryName, brandEn);

                    cars.Add(new Car
                    {
                        BrandAr = brandAr,
                        BrandEn = brandEn,
                        ModelAr = modelAr,
                        ModelEn = modelEn,
                        Year = 2024 - random.Next(0, 3),
                        ColorAr = colorsAr[colorIndex],
                        ColorEn = colorsEn[colorIndex],
                        PlateNumber = $"UAE-{plateCounter++}",
                        SeatingCapacity = seatingCapacity,
                        NumberOfDoors = numberOfDoors,
                        MaxSpeed = maxSpeed,
                        Engine = engine,
                        TransmissionType = categoryName == "Sports" && random.Next(0, 2) == 0 ? TransmissionType.Manual : TransmissionType.Automatic,
                        FuelType = FuelType.Gasoline,
                        DailyRate = basePrice + random.Next(-20, 50),
                        WeeklyRate = (basePrice + random.Next(-20, 50)) * 6,
                        MonthlyRate = (basePrice + random.Next(-20, 50)) * 24,
                        Status = CarStatus.Available,
                        ImageUrl = $"/images/cars/{brandEn.ToLower()}-{modelEn.ToLower().Replace(" ", "-")}-{2024 - random.Next(0, 3)}.jpg",
                        Mileage = random.Next(500, 25000),
                        CategoryId = category.Id,
                        BranchId = branches[random.Next(branches.Count)].Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 365)),
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            // Economy Cars (30+ cars)
            AddCarsForModel("نيسان", "Nissan", "صني", "Sunny", 120.00m, "Economy", 4);
            AddCarsForModel("تويوتا", "Toyota", "يارس", "Yaris", 130.00m, "Economy", 4);
            AddCarsForModel("هيونداي", "Hyundai", "أكسنت", "Accent", 125.00m, "Economy", 3);
            AddCarsForModel("كيا", "Kia", "ريو", "Rio", 115.00m, "Economy", 3);
            AddCarsForModel("شيفروليه", "Chevrolet", "أفيو", "Aveo", 110.00m, "Economy", 3);
            AddCarsForModel("ميتسوبيشي", "Mitsubishi", "أتراج", "Attrage", 125.00m, "Economy", 3);
            AddCarsForModel("هوندا", "Honda", "سيتي", "City", 135.00m, "Economy", 3);
            AddCarsForModel("فولكس واجن", "Volkswagen", "بولو", "Polo", 140.00m, "Economy", 3);
            AddCarsForModel("رينو", "Renault", "لوجان", "Logan", 105.00m, "Economy", 3);
            AddCarsForModel("دايهاتسو", "Daihatsu", "جرانماكس", "Granmax", 100.00m, "Economy", 3);

            // Mid-Size Cars (25+ cars)
            AddCarsForModel("تويوتا", "Toyota", "كامري", "Camry", 200.00m, "Mid-Size", 4);
            AddCarsForModel("هوندا", "Honda", "أكورد", "Accord", 210.00m, "Mid-Size", 4);
            AddCarsForModel("نيسان", "Nissan", "ألتيما", "Altima", 190.00m, "Mid-Size", 3);
            AddCarsForModel("هيونداي", "Hyundai", "سوناتا", "Sonata", 185.00m, "Mid-Size", 3);
            AddCarsForModel("كيا", "Kia", "أوبتيما", "Optima", 180.00m, "Mid-Size", 3);
            AddCarsForModel("فولكس واجن", "Volkswagen", "باسات", "Passat", 220.00m, "Mid-Size", 3);
            AddCarsForModel("شيفروليه", "Chevrolet", "ماليبو", "Malibu", 175.00m, "Mid-Size", 3);
            AddCarsForModel("مازدا", "Mazda", "6", "Mazda6", 195.00m, "Mid-Size", 2);

            // Luxury Cars (20+ cars)
            AddCarsForModel("مرسيدس بنز", "Mercedes-Benz", "إي كلاس", "E-Class", 450.00m, "Luxury", 3);
            AddCarsForModel("بي إم دبليو", "BMW", "الفئة الخامسة", "5 Series", 470.00m, "Luxury", 3);
            AddCarsForModel("أودي", "Audi", "A6", "A6", 460.00m, "Luxury", 3);
            AddCarsForModel("لكزس", "Lexus", "ES", "ES", 430.00m, "Luxury", 3);
            AddCarsForModel("إنفينيتي", "Infiniti", "Q50", "Q50", 420.00m, "Luxury", 2);
            AddCarsForModel("جينيسيس", "Genesis", "G90", "G90", 500.00m, "Luxury", 2);
            AddCarsForModel("كاديلاك", "Cadillac", "CT5", "CT5", 440.00m, "Luxury", 2);
            AddCarsForModel("جاكوار", "Jaguar", "XF", "XF", 480.00m, "Luxury", 2);

            // Sports Cars (15+ cars)
            AddCarsForModel("فورد", "Ford", "موستانغ", "Mustang", 400.00m, "Sports", 3);
            AddCarsForModel("شيفروليه", "Chevrolet", "كامارو", "Camaro", 420.00m, "Sports", 3);
            AddCarsForModel("نيسان", "Nissan", "370زد", "370Z", 380.00m, "Sports", 2);
            AddCarsForModel("بي إم دبليو", "BMW", "Z4", "Z4", 450.00m, "Sports", 2);
            AddCarsForModel("أودي", "Audi", "TT", "TT", 440.00m, "Sports", 2);
            AddCarsForModel("مرسيدس بنز", "Mercedes-Benz", "SLK", "SLK", 470.00m, "Sports", 2);
            AddCarsForModel("بورش", "Porsche", "بوكسترا", "Boxster", 550.00m, "Sports", 1);

            // SUV Cars (30+ cars)
            AddCarsForModel("تويوتا", "Toyota", "برادو", "Prado", 500.00m, "SUV", 4);
            AddCarsForModel("نيسان", "Nissan", "باترول", "Patrol", 550.00m, "SUV", 4);
            AddCarsForModel("تويوتا", "Toyota", "لاند كروزر", "Land Cruiser", 600.00m, "SUV", 3);
            AddCarsForModel("شيفروليه", "Chevrolet", "تاهو", "Tahoe", 520.00m, "SUV", 3);
            AddCarsForModel("فورد", "Ford", "إكسبيديشن", "Expedition", 510.00m, "SUV", 3);
            AddCarsForModel("جي إم سي", "GMC", "يوكن", "Yukon", 530.00m, "SUV", 3);
            AddCarsForModel("كاديلاك", "Cadillac", "إسكاليد", "Escalade", 650.00m, "SUV", 2);
            AddCarsForModel("لينكولن", "Lincoln", "نافيجيتور", "Navigator", 620.00m, "SUV", 2);
            AddCarsForModel("هوندا", "Honda", "بايلوت", "Pilot", 420.00m, "SUV", 3);
            AddCarsForModel("مازدا", "Mazda", "CX-9", "CX-9", 400.00m, "SUV", 2);
            AddCarsForModel("هيونداي", "Hyundai", "باليسيد", "Palisade", 450.00m, "SUV", 2);
            AddCarsForModel("كيا", "Kia", "تيلورايد", "Telluride", 440.00m, "SUV", 2);

            // Set some cars to different statuses for realism
            var totalCars = cars.Count;
            for (int i = 0; i < Math.Min(8, totalCars); i++)
            {
                cars[random.Next(totalCars)].Status = CarStatus.Rented;
            }
            for (int i = 0; i < Math.Min(4, totalCars); i++)
            {
                cars[random.Next(totalCars)].Status = CarStatus.Maintenance;
            }
            for (int i = 0; i < Math.Min(2, totalCars); i++)
            {
                cars[random.Next(totalCars)].Status = CarStatus.OutOfService;
                cars[cars.Count - 1 - i].IsActive = false;
            }

            context.Cars.AddRange(cars);
        } 
    }

        private static string GetEngineSpecification(string categoryName, string brand)
        {
            var random = new Random();

            return categoryName switch
            {
                "Sports" => brand switch
                {
                    "BMW" => "3.0L Twin-Turbo I6",
                    "Mercedes" => "4.0L V8 BiTurbo",
                    "Audi" => "2.9L V6 TFSI",
                    "Porsche" => "3.8L Twin-Turbo Flat-6",
                    "Ferrari" => "3.9L Twin-Turbo V8",
                    "Lamborghini" => "5.2L V10",
                    _ => "3.0L Turbo V6"
                },
                "SUV" => brand switch
                {
                    "BMW" => "3.0L TwinPower Turbo I6",
                    "Mercedes" => "3.0L V6 Turbo",
                    "Audi" => "3.0L TFSI V6",
                    "Toyota" => "3.5L V6",
                    "Lexus" => "3.5L Hybrid V6",
                    "Range Rover" => "3.0L Supercharged V6",
                    "Cadillac" => "6.2L V8",
                    "Lincoln" => "3.0L Twin-Turbo V6",
                    "Infiniti" => "3.5L V6",
                    "Acura" => "3.5L V6",
                    "Volvo" => "2.0L Turbo I4",
                    "Mazda" => "2.5L Turbo I4",
                    "Hyundai" => "3.8L V6",
                    "Kia" => "3.8L V6",
                    _ => "3.0L V6"
                },
                "Luxury" => brand switch
                {
                    "BMW" => "3.0L TwinPower Turbo I6",
                    "Mercedes" => "3.0L V6 Turbo",
                    "Audi" => "3.0L TFSI V6",
                    "Lexus" => "3.5L V6",
                    "Genesis" => "3.3L Twin-Turbo V6",
                    "Cadillac" => "3.6L V6",
                    "Lincoln" => "3.0L Twin-Turbo V6",
                    "Infiniti" => "3.0L Twin-Turbo V6",
                    "Acura" => "3.0L Turbo V6",
                    "Volvo" => "2.0L Turbo I4",
                    _ => "3.0L V6"
                },
                "Compact" => brand switch
                {
                    "BMW" => "2.0L TwinPower Turbo I4",
                    "Mercedes" => "2.0L Turbo I4",
                    "Audi" => "2.0L TFSI I4",
                    "Toyota" => "2.0L I4",
                    "Honda" => "1.5L Turbo I4",
                    "Nissan" => "2.0L I4",
                    "Hyundai" => "2.0L I4",
                    "Kia" => "2.0L I4",
                    "Mazda" => "2.5L I4",
                    "Volkswagen" => "2.0L TSI I4",
                    _ => "2.0L I4"
                },
                _ => "2.0L I4"
            };
        }
 

    private static async Task SeedBookingsAsync(ApplicationDbContext context)
    {
        if (!await context.Bookings.AnyAsync())
        {
            var users = await context.Users.Where(u => u.UserRole == UserRole.Customer).ToListAsync();
            var cars = await context.Cars.Where(c => c.IsActive).ToListAsync();
            var branches = await context.Branches.Where(b => b.IsActive).ToListAsync();
            var random = new Random();

            var bookings = new List<Booking>();
            
            for (int i = 0; i < 25; i++)
            {
                var user = users[random.Next(users.Count)];
                var car = cars[random.Next(cars.Count)];
                var receivingBranch = branches[random.Next(branches.Count)];
                var deliveryBranch = branches[random.Next(branches.Count)];
                
                var startDate = DateTime.UtcNow.AddDays(-random.Next(0, 90)).AddDays(random.Next(1, 30));
                var endDate = startDate.AddDays(random.Next(1, 14));
                var totalDays = (decimal)(endDate - startDate).TotalDays;
                var carRentalCost = totalDays * car.DailyRate;
                
                // Ensure some bookings are completed for loyalty points
                var status = i < 5 ? BookingStatus.Completed : (BookingStatus)random.Next(1, 6);
                var extrasCost = random.Next(0, 200);
                var totalCost = carRentalCost + extrasCost;
                
                bookings.Add(new Booking
                {
                    BookingNumber = $"NOL-{DateTime.UtcNow:yyyyMMdd}-{i + 1:D4}",
                    UserId = user.Id,
                    CarId = car.Id,
                    ReceivingBranchId = receivingBranch.Id,
                    DeliveryBranchId = deliveryBranch.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalDays = totalDays,
                    CarRentalCost = carRentalCost,
                    ExtrasCost = extrasCost,
                    TotalCost = totalCost,
                    DiscountAmount = 0,
                    FinalAmount = totalCost,
                    Status = status,
                    Notes = $"Booking #{i + 1} - Sample booking notes",
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 100)),
                    UpdatedAt = DateTime.UtcNow
                });
            }

            context.Bookings.AddRange(bookings);
        }
    }

    private static async Task SeedReviewsAsync(ApplicationDbContext context)
    {
        if (!await context.Reviews.AnyAsync())
        {
            var bookings = await context.Bookings.Include(b => b.User).Include(b => b.Car).ToListAsync();
            var random = new Random();

            var reviews = new List<Review>();
            var reviewComments = new[]
            {
                "Excellent service and clean car",
                "Very good experience overall", 
                "Satisfied with the rental",
                "Average service quality",
                "Outstanding customer service",
                "Car was in perfect condition",
                "Quick and efficient process",
                "Professional staff and clean vehicle",
                "Great value for money",
                "Will definitely rent again"
            };

            foreach (var booking in bookings.Take(20))
            {
                var comment = reviewComments[random.Next(reviewComments.Length)];
                reviews.Add(new Review
                {
                    UserId = booking.UserId,
                    CarId = booking.CarId,
                    Rating = random.Next(1, 6),
                    Comment = comment,
                    CreatedAt = booking.EndDate.AddDays(random.Next(1, 7))
                });
            }

            context.Reviews.AddRange(reviews);
        }
    }

    private static async Task SeedNotificationsAsync(ApplicationDbContext context)
    {
        if (!await context.Notifications.AnyAsync())
        {
            var users = await context.Users.ToListAsync();
            var random = new Random();

            var notifications = new List<Notification>();
            var notificationTypes = Enum.GetValues<NotificationType>();
            
            var messages = new[]
            {
                ("تم تأكيد حجزك", "Your booking has been confirmed"),
                ("موعد إرجاع السيارة", "Car return reminder"),
                ("عرض خاص", "Special offer available"),
                ("تحديث الحجز", "Booking update"),
                ("رسالة ترحيب", "Welcome message"),
                ("دفعة مستحقة", "Payment due"),
                ("إلغاء الحجز", "Booking cancellation"),
                ("تأكيد الدفع", "Payment confirmation"),
                ("عرض ترويجي", "Promotional offer"),
                ("تذكير بالموعد", "Appointment reminder")
            };

            for (int i = 0; i < 30; i++)
            {
                var user = users[random.Next(users.Count)];
                var (titleAr, titleEn) = messages[random.Next(messages.Length)];
                
                notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Type = notificationTypes[random.Next(notificationTypes.Length)],
                    TitleAr = titleAr,
                    TitleEn = titleEn,
                    MessageAr = $"رسالة تفصيلية #{i + 1} - تفاصيل الإشعار",
                    MessageEn = $"Detailed message #{i + 1} - Notification details",
                    IsRead = random.Next(0, 2) == 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                });
            }

            context.Notifications.AddRange(notifications);
        }
    }

    private static async Task SeedPaymentsAsync(ApplicationDbContext context)
    {
        if (!await context.Payments.AnyAsync())
        {
            var bookings = await context.Bookings.ToListAsync();
            var random = new Random();

            var payments = new List<Payment>();
            var paymentMethods = Enum.GetValues<PaymentMethod>();
            var paymentStatuses = Enum.GetValues<PaymentStatus>();

            foreach (var booking in bookings.Take(20))
            {
                payments.Add(new Payment
                {
                    BookingId = booking.Id,
                    PaymentReference = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{random.Next(1000, 9999)}",
                    Amount = booking.FinalAmount,
                    PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)],
                    Status = paymentStatuses[random.Next(paymentStatuses.Length)],
                    TransactionId = $"TXN_{DateTime.UtcNow.Ticks}_{random.Next(1000, 9999)}",
                    PaymentDate = booking.CreatedAt.AddMinutes(random.Next(5, 60)),
                    Notes = $"Payment for booking {booking.BookingNumber}",
                    CreatedAt = booking.CreatedAt,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            context.Payments.AddRange(payments);
        }
    }

    private static async Task SeedFavoritesAsync(ApplicationDbContext context)
    {
        if (!await context.Favorites.AnyAsync())
        {
            var users = await context.Users.Take(10).ToListAsync();
            var cars = await context.Cars.Take(15).ToListAsync();
            
            var favorites = new List<Favorite>();
            var random = new Random();
            
            // Create some random favorites
            foreach (var user in users)
            {
                var userFavorites = cars.OrderBy(x => random.Next()).Take(random.Next(1, 4));
                foreach (var car in userFavorites)
                {
                    favorites.Add(new Favorite
                    {
                        UserId = user.Id,
                        CarId = car.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90))
                    });
                }
            }
            
            context.Favorites.AddRange(favorites);
        }
    }

    private static async Task SeedLoyaltyPointsAsync(ApplicationDbContext context)
    {
        if (!await context.LoyaltyPointTransactions.AnyAsync())
        {
            var users = await context.Users.Where(u => u.UserRole == UserRole.Customer).Take(10).ToListAsync();
            var bookings = await context.Bookings.Where(b => b.Status == BookingStatus.Completed).ToListAsync();
            
            var loyaltyTransactions = new List<LoyaltyPointTransaction>();
            var random = new Random();
            
            foreach (var user in users)
            {
                // Welcome bonus for each customer
                loyaltyTransactions.Add(new LoyaltyPointTransaction
                {
                    UserId = user.Id,
                    Points = 100,
                    TransactionType = LoyaltyPointTransactionType.Earned,
                    EarnReason = LoyaltyPointEarnReason.Registration,
                    Description = "Welcome bonus for new registration",
                    TransactionDate = user.CreatedAt.AddHours(1),
                    ExpiryDate = user.CreatedAt.AddMonths(24),
                    IsExpired = false,
                    CreatedAt = user.CreatedAt.AddHours(1)
                });

                // Some booking completion points
                var userBookings = bookings.Where(b => b.UserId == user.Id).Take(2);
                foreach (var booking in userBookings)
                {
                    var pointsEarned = (int)Math.Floor(booking.FinalAmount);
                    loyaltyTransactions.Add(new LoyaltyPointTransaction
                    {
                        UserId = user.Id,
                        BookingId = booking.Id,
                        Points = pointsEarned,
                        TransactionType = LoyaltyPointTransactionType.Earned,
                        EarnReason = LoyaltyPointEarnReason.BookingCompleted,
                        Description = $"Points earned for booking completion - ${booking.FinalAmount:F2}",
                        TransactionDate = booking.EndDate.AddHours(2),
                        ExpiryDate = booking.EndDate.AddMonths(24),
                        IsExpired = false,
                        CreatedAt = booking.EndDate.AddHours(2)
                    });
                }

                // Some review points
                if (random.Next(1, 3) == 1) // 33% chance of review points
                {
                    loyaltyTransactions.Add(new LoyaltyPointTransaction
                    {
                        UserId = user.Id,
                        Points = 50,
                        TransactionType = LoyaltyPointTransactionType.Earned,
                        EarnReason = LoyaltyPointEarnReason.Review,
                        Description = "Points for writing a review",
                        TransactionDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                        ExpiryDate = DateTime.UtcNow.AddMonths(24),
                        IsExpired = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                    });
                }

                // Some redemptions for a few users
                if (random.Next(1, 4) == 1) // 25% chance of redemption
                {
                    loyaltyTransactions.Add(new LoyaltyPointTransaction
                    {
                        UserId = user.Id,
                        Points = -100, // Negative for redemption
                        TransactionType = LoyaltyPointTransactionType.Redeemed,
                        Description = "Points redeemed for booking discount",
                        TransactionDate = DateTime.UtcNow.AddDays(-random.Next(1, 15)),
                        IsExpired = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 15))
                    });
                }
            }
            
            context.LoyaltyPointTransactions.AddRange(loyaltyTransactions);

            // Update user loyalty point totals
            foreach (var user in users)
            {
                var userTransactions = loyaltyTransactions.Where(lt => lt.UserId == user.Id).ToList();
                
                var totalEarned = userTransactions
                    .Where(t => t.TransactionType == LoyaltyPointTransactionType.Earned || 
                               t.TransactionType == LoyaltyPointTransactionType.Bonus)
                    .Sum(t => t.Points);
                
                var totalRedeemed = userTransactions
                    .Where(t => t.TransactionType == LoyaltyPointTransactionType.Redeemed)
                    .Sum(t => Math.Abs(t.Points));
                
                var availablePoints = totalEarned - totalRedeemed;
                
                user.TotalLoyaltyPoints = totalEarned;
                user.AvailableLoyaltyPoints = availablePoints;
                user.LifetimePointsEarned = totalEarned;
                user.LifetimePointsRedeemed = totalRedeemed;
                user.LastPointsEarnedDate = userTransactions
                    .Where(t => t.TransactionType == LoyaltyPointTransactionType.Earned)
                    .OrderByDescending(t => t.TransactionDate)
                    .FirstOrDefault()?.TransactionDate;
            }
        }
    }
} 