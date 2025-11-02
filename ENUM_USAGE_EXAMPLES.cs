using NOL.Domain.Enums;
using NOL.Domain.Extensions;

namespace NOL.Examples;

/// <summary>
/// Examples of using the GetDescription() extension method
/// </summary>
public class EnumUsageExamples
{
    public void BasicUsage()
    {
        // ✅ SIMPLE WAY: Get description based on current culture
        var status = BookingStatus.Confirmed;
        var text = status.GetDescription();
        // Returns: "Confirmed" (if culture is "en")
        // Returns: "مؤكد" (if culture is "ar")
        
        Console.WriteLine($"Status: {text}");
    }
    
    public void SpecificCulture()
    {
        // ✅ Get description for specific language
        var status = BookingStatus.Confirmed;
        
        var englishText = status.GetDescription("en");
        Console.WriteLine($"English: {englishText}"); // Output: "Confirmed"
        
        var arabicText = status.GetDescription("ar");
        Console.WriteLine($"Arabic: {arabicText}"); // Output: "مؤكد"
    }
    
    public void InServices()
    {
        // ✅ Use in service methods
        var booking = new Booking 
        { 
            Status = BookingStatus.InProgress 
        };
        
        var message = $"Your booking is {booking.Status.GetDescription()}";
        // English: "Your booking is In Progress"
        // Arabic: "Your booking is قيد التنفيذ"
        
        Console.WriteLine(message);
    }
    
    public void InDTOs()
    {
        // ✅ Use in DTOs
        var dto = new BookingResponseDto
        {
            Id = 123,
            Status = BookingStatus.Completed,
            StatusText = BookingStatus.Completed.GetDescription()
        };
        
        Console.WriteLine($"Booking #{dto.Id}: {dto.StatusText}");
    }
    
    public void DifferentEnumTypes()
    {
        // ✅ Works with ALL enum types
        
        // Car Status
        var carStatus = CarStatus.Available;
        Console.WriteLine(carStatus.GetDescription()); // "Available" or "متاح"
        
        // Fuel Type
        var fuel = FuelType.Electric;
        Console.WriteLine(fuel.GetDescription()); // "Electric" or "كهربائي"
        
        // Transmission
        var transmission = TransmissionType.Automatic;
        Console.WriteLine(transmission.GetDescription()); // "Automatic" or "أوتوماتيك"
        
        // Payment Method
        var payment = PaymentMethod.CreditCard;
        Console.WriteLine(payment.GetDescription()); // "Credit Card" or "بطاقة ائتمان"
        
        // User Role
        var role = UserRole.Admin;
        Console.WriteLine(role.GetDescription()); // "Admin" or "مسؤول"
    }
    
    public void InControllers()
    {
        // ✅ Use in API Controllers
        var booking = GetBooking(123);
        
        var response = new
        {
            BookingId = booking.Id,
            Status = booking.Status,
            StatusText = booking.Status.GetDescription(), // Automatically localized!
            CarStatus = booking.Car.Status.GetDescription(),
            PaymentMethod = booking.PaymentMethod.GetDescription()
        };
        
        // Response will have localized text based on Accept-Language header
    }
    
    public void InEmailTemplates()
    {
        // ✅ Use in email notifications
        var booking = GetBooking(123);
        
        var emailBody = $@"
            Dear Customer,
            
            Your booking status has been updated to: {booking.Status.GetDescription()}
            
            Booking Details:
            - Car Status: {booking.Car.Status.GetDescription()}
            - Payment Method: {booking.PaymentMethod.GetDescription()}
            - Fuel Type: {booking.Car.FuelType.GetDescription()}
            
            Thank you!
        ";
        
        // Email will be in the correct language based on user's culture
    }
    
    public void InValidationMessages()
    {
        // ✅ Use in validation messages
        var booking = GetBooking(123);
        
        if (booking.Status == BookingStatus.Canceled)
        {
            var errorMessage = $"Cannot modify a {booking.Status.GetDescription()} booking";
            // English: "Cannot modify a Canceled booking"
            // Arabic: "Cannot modify a ملغي booking"
            
            throw new InvalidOperationException(errorMessage);
        }
    }
    
    public void ComparingApproaches()
    {
        var status = BookingStatus.Confirmed;
        
        // ✅ SHORT WAY (New!)
        var text1 = status.GetDescription();
        
        // ✅ LONG WAY (Still works!)
        var text2 = status.GetLocalizedDescription();
        
        // Both return the same result!
        // Use whichever you prefer - GetDescription() is shorter!
    }
    
    public void GetAllValues()
    {
        // ✅ Get all enum values with descriptions
        var allStatuses = EnumExtensions.GetEnumList<BookingStatus>();
        
        foreach (var item in allStatuses)
        {
            Console.WriteLine($"{item.Value} - {item.Name} - {item.LocalizedName}");
        }
        // Output (Arabic):
        // 1 - Open - مفتوح
        // 2 - Confirmed - مؤكد
        // 3 - InProgress - قيد التنفيذ
        // 4 - Completed - مكتمل
        // 5 - Canceled - ملغي
        // 6 - Closed - مغلق
    }
    
    // Helper methods for examples
    private Booking GetBooking(int id) => new Booking();
}

// Example models
public class Booking
{
    public int Id { get; set; }
    public BookingStatus Status { get; set; }
    public Car Car { get; set; } = new Car();
    public PaymentMethod PaymentMethod { get; set; }
}

public class Car
{
    public CarStatus Status { get; set; }
    public FuelType FuelType { get; set; }
}

public class BookingResponseDto
{
    public int Id { get; set; }
    public BookingStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
}

