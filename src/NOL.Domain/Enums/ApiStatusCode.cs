using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum ApiStatusCode
{
    [LocalizedDescription("Success", "نجاح")]
    Success = 200,
    
    [LocalizedDescription("Created", "تم الإنشاء")]
    Created = 201,
    
    [LocalizedDescription("No Content", "لا يوجد محتوى")]
    NoContent = 204,
    
    [LocalizedDescription("Bad Request", "طلب خاطئ")]
    BadRequest = 400,
    
    [LocalizedDescription("Unauthorized", "غير مصرح")]
    Unauthorized = 401,
    
    [LocalizedDescription("Forbidden", "محظور")]
    Forbidden = 403,
    
    [LocalizedDescription("Not Found", "غير موجود")]
    NotFound = 404,
    
    [LocalizedDescription("Conflict", "تعارض")]
    Conflict = 409,
    
    [LocalizedDescription("Unprocessable Entity", "كيان غير قابل للمعالجة")]
    UnprocessableEntity = 422,
    
    [LocalizedDescription("Internal Server Error", "خطأ داخلي في الخادم")]
    InternalServerError = 500,
    
    [LocalizedDescription("Service Unavailable", "الخدمة غير متاحة")]
    ServiceUnavailable = 503
} 