using NOL.Domain.Enums;

namespace NOL.Application.DTOs;

public class ExtraDto
{
    public int Id { get; set; }
    public ExtraType ExtraType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DailyPrice { get; set; }
    public decimal WeeklyPrice { get; set; }
    public decimal MonthlyPrice { get; set; }
} 