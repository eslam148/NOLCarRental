using System.Text.Json.Serialization;

namespace NOL.Application.ExternalServices.WASL;

 
public class WaslResponse<T>
{
    public bool Success { get; set; }

     public string ResultCode { get; set; }

     public T Result { get; set; }
}

 
public class WaslHealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}





#region Create Car
public class VehicleRegistrationRequest
{
    
    public string OwnerIdentityNumber { get; set; }

    // تاريخ ميلاد المالك بالتقويم الهجري
    public string OwnerDateOfBirthHijri { get; set; }

    // تاريخ ميلاد المالك بالتقويم الميلادي
    public string OwnerDateOfBirthGregorian { get; set; }

    // الرقم التسلسلي للمركبة
    public string SequenceNumber { get; set; }

    // حرف اللوحة الأيمن
    public string PlateLetterRight { get; set; }

    // حرف اللوحة الأوسط
    public string PlateLetterMiddle { get; set; }

    // حرف اللوحة الأيسر
    public string PlateLetterLeft { get; set; }

    // رقم اللوحة
    public string PlateNumber { get; set; }

    // نوع اللوحة
    public string PlateType { get; set; }
}
public class VehicleRegistrationResponse
{
   
        // حالة الأهلية (افتراض: قيمة منطقية true/false)
        public bool Eligibility { get; set; }

        // قائمة بأسباب الرفض (إذا كان هناك رفض)
        public List<string> RejectionReasons { get; set; }
}
#endregion

#region  VehicleEligibility

public class VehicleEligibilityRespone
{
    // حالة أهلية المركبة (مثلاً: VALID)
    public string VehicleEligibilityStatus { get; set; }

    // وقت تسجيل المركبة (سيتم تحويلها إلى DateTime)
    public DateTime RegisterationTime { get; set; }

    // تاريخ انتهاء صلاحية بطاقة التشغيل (سيتم تحويلها إلى DateTime)
    public DateTime OperatingCardExpiryDate { get; set; }

    // سبب الرفض (إذا كانت غير صالحة)
    public string RejectionReasons { get; set; }
}


#endregion

#region RentalOperation

public class RentalOperationResponse
{
    // مؤشر نجاح العملية (قيمة منطقية)
    public bool Success { get; set; }

    // رمز نتيجة العملية
    public string ResultCode { get; set; }
}
public class RentalOperationRequest
{
    // الرقم التسلسلي للطلب
    public string SequenceNumber { get; set; }

    // معرف عملية تأجير الشركة
    public string CompanyRentalOperationId { get; set; }

    // خط عرض نقطة الاستلام
    public double PickupLatitude { get; set; }

    // خط طول نقطة الاستلام
    public double PickupLongitude { get; set; }

    // خط عرض نقطة الإنزال
    public double DropoffLatitude { get; set; }

    // خط طول نقطة الإنزال
    public double DropoffLongitude { get; set; }

    // طابع وقت الاستلام (ISO 8601)
    public DateTime PickupTimeStamp { get; set; }

    // طابع وقت الإنزال (ISO 8601)
    public DateTime DropoffTimeStamp { get; set; }

    // فترة شغل المركبة بالتأجير بالدقائق
    public int RentalVehicleOccupationPeriodInMinutes { get; set; }

    // تقييم العميل للمركبة
    public int CustomerVehicleRating { get; set; }

    // تقييم العميل للخدمة
    public int CustomerServiceRating { get; set; }
}



#endregion

#region Locations

public class LocationUpdate
{
    // الرقم التسلسلي للمركبة
    public string VehicleSequenceNumber { get; set; }

    // خط العرض
    public double Latitude { get; set; }

    // خط الطول
    public double Longitude { get; set; }

    // وقت آخر تحديث للموقع (ISO 8601)
    public DateTime UpdatedWhen { get; set; }

    // مؤشر لوجود عميل (0: لا يوجد، 1: يوجد)
    // يمكن استخدام int أو bool (إذا كان 0/1 فقط)
    public int HasCustomer { get; set; }
}

public class LocationRequest
{
    // قائمة بتحديثات مواقع المركبات
    public List<LocationUpdate> Locations { get; set; }
}



public class FailedVehicle
{
    // الرقم التسلسلي للمركبة التي فشلت
    public string SequenceNumber { get; set; }

    // سبب الفشل
    public string Reason { get; set; }
}

public class LocationResposne
{
    // قائمة بالمركبات التي فشلت في المعالجة
    public List<FailedVehicle> FailedVehicles { get; set; }
}


#endregion

 