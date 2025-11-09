using NOL.Domain.Attributes;

namespace NOL.Application.Common.Responses;

public enum ResponseCode
{
    [LocalizedDescription("لا شيء", "None")]
    None = 0,
    
    [LocalizedDescription("خطأ داخلي في الخادم", "Internal server error")]
    InternalServerError = 1,
    
    [LocalizedDescription("نطاق التاريخ غير صالح", "Invalid date range")]
    InvalidDateRange = 2,
    
    [LocalizedDescription("خطأ في التحقق من الصحة", "Validation error")]
    ValidationError = 3,
    
    [LocalizedDescription("تمت العملية بنجاح", "Operation successful")]
    OperationSuccessful = 4,
    
    [LocalizedDescription("إحداثيات غير صالحة", "Invalid coordinates")]
    InvalidCoordinates = 5,
    
    [LocalizedDescription("غير مطبق", "Not implemented")]
    NotImplemented = 6,
    
    [LocalizedDescription("طلب غير صالح", "Invalid request")]
    InvalidRequest = 7,
    
    [LocalizedDescription("غير مصرح", "Unauthorized")]
    Unauthorized = 8,
    
    [LocalizedDescription("المورد غير موجود", "Resource not found")]
    ResourceNotFound = 9,
    
    [LocalizedDescription("العملية غير صالحة", "Operation not valid")]
    OperationNotValid = 10,
    
    [LocalizedDescription("الميزة غير مطبقة", "Feature not implemented")]
    FeatureNotImplemented = 11,
    
    [LocalizedDescription("انتهت مهلة الطلب", "Request timeout")]
    RequestTimeout = 12,
    
    #region Advertisements

    [LocalizedDescription("تم استرجاع الإعلانات بنجاح", "Advertisements retrieved successfully")]
    AdvertisementsRetrieved = 100,
    
    [LocalizedDescription("تم استرجاع الإعلانات المميزة بنجاح", "Featured advertisements retrieved successfully")]
    FeaturedAdvertisementsRetrieved = 101,
    
    [LocalizedDescription("الإعلان غير موجود", "Advertisement not found")]
    AdvertisementNotFound = 102,
    
    [LocalizedDescription("لا يمكن تعيين كل من السيارة والفئة", "Cannot set both car and category")]
    CannotSetBothCarAndCategory = 103,
    
    [LocalizedDescription("يجب تعيين السيارة أو الفئة", "Must set either car or category")]
    MustSetEitherCarOrCategory = 104,
    
    [LocalizedDescription("تم إنشاء الإعلان بنجاح", "Advertisement created successfully")]
    AdvertisementCreated = 105,
    
    [LocalizedDescription("تم تحديث الإعلان بنجاح", "Advertisement updated successfully")]
    AdvertisementUpdated = 106,
    
    [LocalizedDescription("تم حذف الإعلان بنجاح", "Advertisement deleted successfully")]
    AdvertisementDeleted = 107,
    
    [LocalizedDescription("تم زيادة عدد المشاهدات", "View count incremented")]
    ViewCountIncremented = 108,
    
    [LocalizedDescription("تم زيادة عدد النقرات", "Click count incremented")]
    ClickCountIncremented = 109,
    
    [LocalizedDescription("تم تحديث حالة الإعلان", "Advertisement status updated")]
    AdvertisementStatusUpdated = 110,
    #endregion

    #region Auth

    [LocalizedDescription("البريد الإلكتروني أو كلمة المرور غير صحيحة", "Invalid email or password")]
    InvalidEmailOrPassword = 500,
    
    [LocalizedDescription("البريد الإلكتروني غير مفعل", "Email not verified")]
    EmailNotVerified = 501,
    
    [LocalizedDescription("تم تسجيل الدخول بنجاح", "Login successful")]
    LoginSuccessful = 502,
    
    [LocalizedDescription("كلمة المرور مطلوبة", "Password required")]
    PasswordRequired = 503,
    
    [LocalizedDescription("كلمات المرور غير متطابقة", "Passwords do not match")]
    PasswordsDoNotMatch = 504,
    
    [LocalizedDescription("البريد الإلكتروني مستخدم بالفعل", "Email already exists")]
    EmailAlreadyExists = 505,
    
    [LocalizedDescription("المستخدم غير موجود", "User not found")]
    UserNotFound = 506,
    
    [LocalizedDescription("البريد الإلكتروني مفعل بالفعل", "Email already confirmed")]
    EmailAlreadyConfirmed = 507,
    
    [LocalizedDescription("تم تسجيل المستخدم بنجاح", "User registered successfully")]
    UserRegistered = 508,
    
    [LocalizedDescription("تم إرسال رمز التحقق من البريد الإلكتروني", "Email verification code sent")]
    EmailVerificationSent = 509,
    
    [LocalizedDescription("انتهت صلاحية رمز التحقق", "OTP expired")]
    OtpExpired = 510,
    
    [LocalizedDescription("رمز التحقق غير صحيح", "Invalid OTP")]
    InvalidOtp = 511,
    
    [LocalizedDescription("تم تفعيل البريد الإلكتروني بنجاح", "Email verified successfully")]
    EmailVerified = 512,
    
    [LocalizedDescription("تم إرسال رابط إعادة تعيين كلمة المرور", "Password reset email sent")]
    PasswordResetEmailSent = 513,
    
    [LocalizedDescription("فشل في إعادة تعيين كلمة المرور", "Password reset failed")]
    PasswordResetFailed = 514,
    
    [LocalizedDescription("تم إعادة تعيين كلمة المرور بنجاح", "Password reset successful")]
    PasswordResetSuccessful = 515,
    
    [LocalizedDescription("فشل في تغيير كلمة المرور", "Password change failed")]
    PasswordChangeFailed = 516,
    
    [LocalizedDescription("تم تغيير كلمة المرور بنجاح", "Password changed successfully")]
    PasswordChanged = 517,
    
    [LocalizedDescription("كلمة المرور غير صحيحة", "Invalid password")]
    InvalidPassword = 518,
    
    [LocalizedDescription("فشل في إرسال البريد الإلكتروني", "Email sending failed")]
    EmailSendingFailed = 519,
    
    [LocalizedDescription("تم إرسال رمز حذف الحساب", "Account deletion OTP sent")]
    AccountDeletionOtpSent = 520,
    
    [LocalizedDescription("نص التأكيد غير صحيح", "Invalid confirmation text")]
    InvalidConfirmationText = 521,
    
    [LocalizedDescription("فشل في حذف الحساب", "Account deletion failed")]
    AccountDeletionFailed = 522,
    
    [LocalizedDescription("تم حذف الحساب بنجاح", "Account deleted successfully")]
    AccountDeletedSuccessfully = 523,
    
    [LocalizedDescription("عدد كبير جداً من محاولات إعادة الإرسال", "Too many resend attempts")]
    TooManyResendAttempts = 524,
    
    [LocalizedDescription("لا يوجد طلب حذف", "No deletion request found")]
    NoDeletionRequestFound = 525,
    
    [LocalizedDescription("تم إعادة إرسال رمز حذف الحساب", "Account deletion OTP resent")]
    AccountDeletionOtpResent = 526,
    
    [LocalizedDescription("فشل في تحديث الملف الشخصي", "Profile update failed")]
    ProfileUpdateFailed = 527,
    
    [LocalizedDescription("تم تحديث الملف الشخصي بنجاح", "Profile updated successfully")]
    ProfileUpdatedSuccessfully = 528,
    #endregion

    #region Bookings

    [LocalizedDescription("تم استرجاع الحجوزات بنجاح", "Bookings retrieved successfully")]
    BookingsRetrieved = 1000,
    
    [LocalizedDescription("الحجز غير موجود", "Booking not found")]
    BookingNotFound = 1001,
    
    [LocalizedDescription("تم إنشاء الحجز بنجاح", "Booking created successfully")]
    BookingCreated = 1002,
    
    [LocalizedDescription("غير مصرح لك بتعديل هذا الحجز", "Unauthorized to modify booking")]
    UnauthorizedToModifyBooking = 1003,
    
    [LocalizedDescription("لا يمكن إلغاء هذا الحجز", "Booking cannot be canceled")]
    BookingCannotBeCanceled = 1004,
    
    [LocalizedDescription("لا يمكن إلغاء حجز قد بدأ", "Cannot cancel started booking")]
    CannotCancelStartedBooking = 1005,
    
    [LocalizedDescription("تم إلغاء الحجز بنجاح", "Booking canceled successfully")]
    BookingCanceledSuccessfully = 1006,
    #endregion

    #region Car
    
    [LocalizedDescription("السيارة غير موجودة", "Car not found")]
    CarNotFound = 1500,
    
    [LocalizedDescription("السيارة غير متاحة", "Car not available")]
    CarNotAvailable = 1501,
    
    [LocalizedDescription("تم استرجاع السيارات بنجاح", "Cars retrieved successfully")]
    CarsRetrieved = 1502,
    
    [LocalizedDescription("تم استرجاع أسعار السيارة", "Car rates retrieved successfully")]
    CarRatesRetrieved = 1503,
    #endregion

    #region Branch
    
    [LocalizedDescription("فرع الاستلام غير متاح", "Receiving branch not available")]
    ReceivingBranchNotAvailable = 2000,
    
    [LocalizedDescription("فرع التسليم غير متاح", "Delivery branch not available")]
    DeliveryBranchNotAvailable = 2001,
    
    [LocalizedDescription("تم استرجاع الفروع بنجاح", "Branches retrieved successfully")]
    BranchesRetrieved = 2002,
    
    [LocalizedDescription("الفرع غير موجود", "Branch not found")]
    BranchNotFound = 2003,
    
    [LocalizedDescription("تم استرجاع الفروع القريبة بنجاح", "Nearby branches retrieved successfully")]
    NearbyBranchesRetrieved = 2004,
    #endregion

    #region Categories

    [LocalizedDescription("تم استرجاع الفئات بنجاح", "Categories retrieved successfully")]
    CategoriesRetrieved = 2500,
    
    [LocalizedDescription("الفئة غير موجودة", "Category not found")]
    CategoryNotFound = 2501,
    
    [LocalizedDescription("تم استرجاع الفئة بنجاح", "Category retrieved successfully")]
    CategoryRetrieved = 2502,
    #endregion

    #region Contact
    
    [LocalizedDescription("لا توجد معلومات اتصال نشطة", "No active contact us found")]
    NoActiveContactUsFound = 3000,
    
    [LocalizedDescription("تم استرجاع معلومات الاتصال النشطة", "Active contact us retrieved successfully")]
    ActiveContactUsRetrieved = 3001,
    
    [LocalizedDescription("تم استرجاع قائمة معلومات الاتصال", "Contact us list retrieved successfully")]
    ContactUsListRetrieved = 3002,
    
    [LocalizedDescription("تم إنشاء معلومات الاتصال بنجاح", "Contact us created successfully")]
    ContactUsCreated = 3003,
    
    [LocalizedDescription("تم تحديث معلومات الاتصال بنجاح", "Contact us updated successfully")]
    ContactUsUpdated = 3004,
    
    [LocalizedDescription("تم حذف معلومات الاتصال بنجاح", "Contact us deleted successfully")]
    ContactUsDeleted = 3005,
    
    [LocalizedDescription("فشل حذف معلومات الاتصال", "Contact us deletion failed")]
    ContactUsDeletionFailed = 3006,
    
    [LocalizedDescription("فشل تفعيل معلومات الاتصال", "Contact us activation failed")]
    ContactUsActivationFailed = 3007,
    #endregion

    #region LoyaltyPoints

    [LocalizedDescription("تم استرجاع ملخص نقاط الولاء", "Loyalty points summary retrieved successfully")]
    LoyaltyPointSummaryRetrieved = 3500,
    
    [LocalizedDescription("تم استرجاع معاملات نقاط الولاء", "Loyalty points transactions retrieved successfully")]
    LoyaltyPointTransactionsRetrieved = 3501,
    
    [LocalizedDescription("تم منح نقاط الولاء بنجاح", "Loyalty points awarded successfully")]
    LoyaltyPointsAwarded = 3502,
    
    [LocalizedDescription("لم يتم تحقيق الحد الأدنى للاسترداد", "Minimum redemption not met")]
    MinimumRedemptionNotMet = 3503,
    
    [LocalizedDescription("نقاط الولاء غير كافية", "Insufficient loyalty points")]
    InsufficientLoyaltyPoints = 3504,
    
    [LocalizedDescription("تم استرداد نقاط الولاء بنجاح", "Loyalty points redeemed successfully")]
    LoyaltyPointsRedeemed = 3505,
    
    [LocalizedDescription("تم منح النقاط مسبقاً", "Points already awarded")]
    PointsAlreadyAwarded = 3506,
    
    [LocalizedDescription("تمت معالجة نقاط الحجز", "Booking points processed successfully")]
    BookingPointsProcessed = 3507,
    
    [LocalizedDescription("تمت معالجة النقاط المنتهية", "Expired points processed successfully")]
    ExpiredPointsProcessed = 3508,
    #endregion

    #region Extra Service 

    [LocalizedDescription("تم استرجاع الخدمات الإضافية بنجاح", "Extra services retrieved successfully")]
    ExtrasRetrieved = 4000,
    
    [LocalizedDescription("الخدمة الإضافية غير موجودة", "Extra service not found")]
    ExtraServiceNotFound = 4001,
    
    [LocalizedDescription("تم استرجاع الخدمة الإضافية بنجاح", "Extra service retrieved successfully")]
    ExtraRetrieved = 4002,
    #endregion

    #region Favorites
    
    [LocalizedDescription("تم استرجاع المفضلة بنجاح", "Favorites retrieved successfully")]
    FavoritesRetrieved = 4500,
    
    [LocalizedDescription("السيارة موجودة بالفعل في المفضلة", "Car already in favorites")]
    CarAlreadyInFavorites = 4501,
    
    [LocalizedDescription("تمت إضافة السيارة إلى المفضلة", "Favorite added successfully")]
    FavoriteAdded = 4502,
    
    [LocalizedDescription("المفضلة غير موجودة", "Favorite not found")]
    FavoriteNotFound = 4503,
    
    [LocalizedDescription("تم تحديث المفضلة بنجاح", "Favorite updated successfully")]
    FavoriteUpdated = 4504,
    
    [LocalizedDescription("تمت إزالة السيارة من المفضلة", "Favorite removed successfully")]
    FavoriteRemoved = 4505,
    
    [LocalizedDescription("تم استرجاع حالة المفضلة", "Favorite status retrieved successfully")]
    FavoriteStatusRetrieved = 4506,
    #endregion

    #region Reviews
    
    [LocalizedDescription("تم استرجاع التقييمات بنجاح", "Reviews retrieved successfully")]
    ReviewsRetrieved = 5000,
    
    [LocalizedDescription("تم استرجاع تقييمات المستخدم بنجاح", "User reviews retrieved successfully")]
    UserReviewsRetrieved = 5001,
    
    [LocalizedDescription("التقييم غير موجود", "Review not found")]
    ReviewNotFound = 5002,
    
    [LocalizedDescription("تم استرجاع تقييم السيارة", "Car rating retrieved successfully")]
    CarRatingRetrieved = 5003,
    
    [LocalizedDescription("يمكنك كتابة تقييم", "Can review")]
    CanReview = 5004,
    
    [LocalizedDescription("يجب إكمال الحجز أولاً لكتابة تقييم", "Must complete booking to review")]
    MustCompleteBookingToReview = 5005
    #endregion
}