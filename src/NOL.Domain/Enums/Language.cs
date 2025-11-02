using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum Language
{
    [LocalizedDescription("Arabic", "العربية")]
    Arabic = 1,
    
    [LocalizedDescription("English", "الإنجليزية")]
    English = 2
} 