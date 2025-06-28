# NOL Car Rental System

A comprehensive car rental mobile application backend using ASP.NET Core Web API with Clean Architecture, featuring complete Arabic and English localization support.

## 🚀 Features

- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and API layers
- **Bilingual Support**: Complete Arabic and English localization
- **JWT Authentication**: Secure token-based authentication with Microsoft Identity
- **Generic API Response**: Consistent, localized API responses
- **Type-Safe Enums**: All status and type fields use strongly-typed enums
- **Entity Framework Core**: Code-first approach with comprehensive relationships
- **Swagger Documentation**: Complete API documentation with JWT support
- **Production Ready**: Security, validation, error handling, and logging

## 🏗️ Architecture

```
src/
├── NOL.API/                    # Web API Layer
│   ├── Controllers/            # API Controllers
│   ├── Middleware/            # Custom Middleware
│   ├── Resources/             # Localization Files
│   └── Program.cs             # App Configuration
├── NOL.Application/           # Application Layer
│   ├── Common/               # Shared Components
│   │   ├── Interfaces/       # Service Interfaces
│   │   ├── Models/          # Common Models
│   │   ├── Responses/       # API Response Models
│   │   ├── Services/        # Application Services
│   │   └── Enums/          # Application Enums
│   ├── Features/            # Feature-based Organization
│   └── DTOs/               # Data Transfer Objects
├── NOL.Domain/               # Domain Layer
│   ├── Entities/            # Domain Entities
│   └── Enums/              # Domain Enums
└── NOL.Infrastructure/       # Infrastructure Layer
    ├── Data/               # DbContext & Migrations
    ├── Services/           # Service Implementations
    └── Repositories/       # Data Access
```

## 🔧 Technology Stack

- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: Microsoft Identity with JWT tokens
- **Localization**: Arabic and English support
- **Mapping**: AutoMapper
- **Validation**: FluentValidation
- **Documentation**: Swagger/OpenAPI

## 📦 Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or SQL Server instance)
- Visual Studio 2022 or VS Code

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd NOL-Car-Rental-System
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Update Database Connection

Update the connection string in `src/NOL.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server Connection String"
  }
}
```

### 4. Create Database

The application will automatically create the database on startup. Alternatively, you can use Entity Framework migrations:

```bash
cd src/NOL.API
dotnet ef database update
```

### 5. Run the Application

```bash
cd src/NOL.API
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001` (Development only)

## 🌍 Localization

The system supports Arabic and English languages:

### Language Detection Order:
1. User's preferred language (from JWT token)
2. `Accept-Language` header
3. Default: Arabic (`ar`)

### Switching Languages:
- **Query Parameter**: `?culture=en` or `?culture=ar`
- **Header**: `Accept-Language: en` or `Accept-Language: ar`
- **User Preference**: Set during registration/login

## 🔐 Authentication

### Register a New User

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123",
  "confirmPassword": "Password123",
  "firstName": "احمد",
  "lastName": "محمد",
  "preferredLanguage": 1
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123"
}
```

### Using JWT Token

Include the token in the Authorization header:

```http
Authorization: Bearer <your-jwt-token>
```

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout

### Cars
- `GET /api/cars` - Get cars list with filtering
- `GET /api/cars/{id}` - Get car by ID

### Query Parameters for Cars:
- `status` - Filter by car status (Available, Rented, Maintenance, OutOfService)
- `categoryId` - Filter by category
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 10)

## 🏗️ Core Enums

The system uses strongly-typed enums for all status and type fields:

- **CarStatus**: Available, Rented, Maintenance, OutOfService
- **BookingStatus**: Open, Confirmed, InProgress, Completed, Canceled, Closed
- **PaymentStatus**: Pending, Processing, Success, Failed, Canceled, Refunded
- **PaymentMethod**: Cash, CreditCard, DebitCard, BankTransfer, DigitalWallet, ApplePay, GooglePay, STCPay
- **Language**: Arabic, English
- **UserRole**: Customer, Employee, BranchManager, Admin, SuperAdmin
- **NotificationType**: Booking, Payment, General, Promotion, Maintenance, Security
- **ExtraType**: GPS, ChildSeat, AdditionalDriver, Insurance, WifiHotspot, etc.

## 📊 Generic API Response

All API responses follow a consistent pattern:

```json
{
  "succeeded": true,
  "message": "تم استرداد السيارات بنجاح",
  "data": [...],
  "errors": [],
  "statusCode": 200,
  "statusCodeValue": 200
}
```

## 🗄️ Database Schema

### Core Entities:
- **ApplicationUser** (extends IdentityUser)
- **Branch** - Rental locations
- **Category** - Car categories
- **Car** - Vehicle information
- **Booking** - Rental bookings
- **Payment** - Payment records
- **ExtraTypePrice** - Additional services pricing
- **Favorite** - User favorites
- **Review** - Car reviews
- **Notification** - User notifications
- **SystemSettings** - Application settings

## 🔧 Configuration

### JWT Settings (`appsettings.json`):
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "NOLCarRentalAPI",
    "Audience": "NOLCarRentalApp",
    "ExpiryInMinutes": 60
  }
}
```

### Localization Settings:
```json
{
  "Localization": {
    "DefaultCulture": "ar",
    "SupportedCultures": ["ar", "en"]
  }
}
```

## 🧪 Development

### Add New Entity:
1. Create entity in `NOL.Domain/Entities/`
2. Add DbSet to `ApplicationDbContext`
3. Configure relationships in `OnModelCreating`
4. Create DTO in `NOL.Application/DTOs/`
5. Add controller in `NOL.API/Controllers/`

### Add New Localization:
1. Add keys to `SharedResource.ar.resx` and `SharedResource.en.resx`
2. Use in controllers via `ILocalizationService` or `LocalizedApiResponseService`

## 🚀 Deployment

1. Update connection strings for production
2. Update JWT secret key
3. Configure CORS settings
4. Set up SSL certificates
5. Configure logging providers

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 📞 Support

For support and questions, please contact the development team.

---

**NOL Car Rental System** - A modern, scalable, and localized car rental platform built with ASP.NET Core 8.0 