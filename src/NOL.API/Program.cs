using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NOL.API.Middleware;
using NOL.API.Resources;
using NOL.API.Services;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Models;
using NOL.Application.Common.Services;
using NOL.Application.Features.Advertisements;
using NOL.Application.Features.Bookings;
using NOL.Application.Features.Branches;
using NOL.Application.Features.Cars;
using NOL.Application.Features.Categories;
using NOL.Application.Features.Extras;
using NOL.Application.Features.Favorites;
using NOL.Application.Features.LoyaltyPoints;
using NOL.Domain.Entities;
using NOL.Infrastructure.Data;
using NOL.Infrastructure.Repositories;
using NOL.Infrastructure.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true; // Require email confirmation for sign-in
     
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings!.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
    };
});

// Localization Configuration
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "ar" };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    
    // Configure culture providers with proper fallback
    options.RequestCultureProviders.Clear();
    
    // Query string provider (culture=ar, ui-culture=ar)
    options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider
    {
        QueryStringKey = "culture",
        UIQueryStringKey = "ui-culture"
    });
    
    // Header provider for Accept-Language
    options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
    
    // Cookie provider for persisting culture
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
});

// Services Registration  
builder.Services.AddScoped<ILocalizationService, NOL.API.Services.LocalizationService>();
builder.Services.AddScoped<LocalizedApiResponseService>();
builder.Services.AddScoped<IAuthService, NOL.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<IEmailService, NOL.Infrastructure.Services.EmailService>();

// Repository Layer
builder.Services.AddScoped(typeof(IRepository<>), typeof(NOL.Infrastructure.Repositories.Repository<>));
builder.Services.AddScoped<ICarRepository, NOL.Infrastructure.Repositories.CarRepository>();
builder.Services.AddScoped<IBranchRepository, NOL.Infrastructure.Repositories.BranchRepository>();
builder.Services.AddScoped<ICategoryRepository, NOL.Infrastructure.Repositories.CategoryRepository>();
builder.Services.AddScoped<IExtraRepository, NOL.Infrastructure.Repositories.ExtraRepository>();
builder.Services.AddScoped<IBookingRepository, NOL.Infrastructure.Repositories.BookingRepository>();
builder.Services.AddScoped<IAdvertisementRepository, NOL.Infrastructure.Repositories.AdvertisementRepository>();
builder.Services.AddScoped<ILoyaltyPointRepository, NOL.Infrastructure.Repositories.LoyaltyPointRepository>();
builder.Services.AddScoped<IFavoriteRepository, NOL.Infrastructure.Repositories.FavoriteRepository>();
builder.Services.AddScoped<IReviewRepository, NOL.Infrastructure.Repositories.ReviewRepository>();
builder.Services.AddScoped<IContactUsRepository, NOL.Infrastructure.Repositories.ContactUsRepository>();

// Application Layer Services
builder.Services.AddScoped<ICarService, NOL.Application.Features.Cars.CarService>();
builder.Services.AddScoped<IBranchService, NOL.Application.Features.Branches.BranchService>();
builder.Services.AddScoped<ICategoryService, NOL.Application.Features.Categories.CategoryService>();
builder.Services.AddScoped<IExtraService, NOL.Application.Features.Extras.ExtraService>();
builder.Services.AddScoped<IBookingService, NOL.Application.Features.Bookings.BookingService>();
builder.Services.AddScoped<IAdvertisementService, NOL.Application.Features.Advertisements.AdvertisementService>();
builder.Services.AddScoped<ILoyaltyPointService, NOL.Application.Features.LoyaltyPoints.LoyaltyPointService>();
builder.Services.AddScoped<IFavoriteService, NOL.Application.Features.Favorites.FavoriteService>();
builder.Services.AddScoped<IRateCalculationService, NOL.Application.Features.RateCalculation.RateCalculationService>();
builder.Services.AddScoped<IReviewService, NOL.Application.Features.Reviews.ReviewService>();
builder.Services.AddScoped<IContactUsService, NOL.Application.Features.ContactUs.ContactUsService>();
builder.Services.AddScoped<ICarService, NOL.Application.Features.Cars.CarService>();
builder.Services.AddScoped<IAdminManagementService, NOL.Infrastructure.Services.AdminManagementService>();
builder.Services.AddScoped<ICarManagementService, NOL.Infrastructure.Services.CarManagementService>();
builder.Services.AddScoped<IBookingManagementService, NOL.Infrastructure.Services.BookingManagementService>();
builder.Services.AddScoped<ICustomerManagementService, NOL.Infrastructure.Services.CustomerManagementService>();
builder.Services.AddScoped<IAdvertisementManagementService, NOL.Infrastructure.Services.AdvertisementManagementService>();
builder.Services.AddScoped<IBranchManagementService, NOL.Infrastructure.Services.BranchManagementService>();
builder.Services.AddScoped<IDashboardService, NOL.Infrastructure.Services.DashboardService>();
builder.Services.AddScoped<ISystemManagementService, NOL.Infrastructure.Services.SystemManagementService>();
builder.Services.AddScoped<IExtraTypePriceManagementService, NOL.Infrastructure.Services.ExtraTypePriceManagementService>();

// Additional Management Services (Interfaces created, implementations can be added later)
// builder.Services.AddScoped<INotificationManagementService, NOL.Infrastructure.Services.NotificationManagementService>();
// builder.Services.AddScoped<IAuditLogService, NOL.Infrastructure.Services.AuditLogService>();
// builder.Services.AddScoped<IMaintenanceManagementService, NOL.Infrastructure.Services.MaintenanceManagementService>();
// builder.Services.AddScoped<IReportManagementService, NOL.Infrastructure.Services.ReportManagementService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON to serialize enums as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NOL Car Rental API",
        Version = "v1",
        Description = "A comprehensive car rental system API with Arabic and English localization support"
    });

    // Configure Swagger to show enums as strings
    c.SchemaFilter<EnumSchemaFilter>();

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NOL Car Rental API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
//}

app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

// Add RequestLocalization middleware (this should come before custom middleware)
app.UseRequestLocalization();

// Custom Culture Middleware (for additional logic if needed)
app.UseMiddleware<CultureMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created, migrated, and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    await context.Database.EnsureCreatedAsync();
    
    // Seed the database with initial data
    await DataSeeder.SeedAsync(context, userManager, roleManager);
}

app.Run();

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            Enum.GetNames(context.Type)
                .ToList()
                .ForEach(name => schema.Enum.Add(new OpenApiString(name)));
        }
    }
} 