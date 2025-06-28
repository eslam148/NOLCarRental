using NOL.Domain.Enums;

namespace NOL.Domain.Entities;

public class ExtraTypePrice
{
    public int Id { get; set; }
    public ExtraType ExtraType { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public decimal DailyPrice { get; set; }
    public decimal WeeklyPrice { get; set; }
    public decimal MonthlyPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<BookingExtra> BookingExtras { get; set; } = new List<BookingExtra>();
} 