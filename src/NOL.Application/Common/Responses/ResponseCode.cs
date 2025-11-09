using NOL.Domain.Attributes;

namespace NOL.Application.Common.Responses;

public enum ResponseCode
{
    [LocalizedDescription("لا شئ,","None")]
    None = 0,
    InternalServerError = 1,
    InvalidDateRange = 2,
    ValidationError = 3,
    OperationSuccessful = 4,
    InvalidCoordinates = 5,
    NotImplemented = 6,
     #region AdvertisementsRetrieved

    AdvertisementsRetrieved =100,
    FeaturedAdvertisementsRetrieved = 101,
    AdvertisementNotFound = 102,
    CannotSetBothCarAndCategory=103,
    MustSetEitherCarOrCategory =104,
    AdvertisementCreated = 105,
    AdvertisementUpdated  = 106,
    AdvertisementDeleted = 107,
    ViewCountIncremented  = 108,
    ClickCountIncremented  = 109,
    AdvertisementStatusUpdated  = 110,
    #endregion

    #region Auth

    InvalidEmailOrPassword = 500,
    EmailNotVerified = 501,
    LoginSuccessful = 502,
    PasswordRequired  = 503,
    PasswordsDoNotMatch =504,
    EmailAlreadyExists  = 505,
    UserNotFound  = 506,
    EmailAlreadyConfirmed = 507,
    UserRegistered = 508,
    EmailVerificationSent  = 509,
    OtpExpired = 510,
    InvalidOtp = 511,
    EmailVerified = 512,
    PasswordResetEmailSent = 513,
    PasswordResetFailed = 514,
    PasswordResetSuccessful = 515,
    PasswordChangeFailed = 516,
    PasswordChanged = 517,
    InvalidPassword = 518,
    EmailSendingFailed = 519,
    AccountDeletionOtpSent = 520,
    InvalidConfirmationText = 521,
    AccountDeletionFailed = 522,
    AccountDeletedSuccessfully = 523,
    TooManyResendAttempts = 524,
    NoDeletionRequestFound = 525,
    AccountDeletionOtpResent = 526,
    ProfileUpdateFailed = 527,
    ProfileUpdatedSuccessfully = 528,
    #endregion


    #region  Bookings

    BookingsRetrieved = 1000,
    BookingNotFound = 1001,
    BookingCreated = 1002,
    UnauthorizedToModifyBooking = 1003,
    BookingCannotBeCanceled = 1004,
    CannotCancelStartedBooking = 1005,
    BookingCanceledSuccessfully = 1006,
    
    #endregion

    #region Car
    CarNotFound =  1500,
    CarNotAvailable = 1501,
    CarsRetrieved = 1502,
    CarRatesRetrieved = 1503,
    #endregion

    #region Branch
    ReceivingBranchNotAvailable = 2000,
    DeliveryBranchNotAvailable = 2001,
    BranchesRetrieved =  2002,
    BranchNotFound = 2003,
    NearbyBranchesRetrieved = 2004,

    #endregion

    #region  Categories

    CategoriesRetrieved = 2500,
    CategoryNotFound = 2501,
    CategoryRetrieved = 2502,
    #endregion

    #region Contact
    NoActiveContactUsFound = 3000,
    ActiveContactUsRetrieved = 3001,
    ContactUsListRetrieved = 3002,
    ContactUsCreated = 3003,
    ContactUsUpdated = 3004,
    ContactUsDeleted= 3005,
    ContactUsDeletionFailed = 3006,
    ContactUsActivationFailed = 3007,
    #endregion

    #region LoyaltyPoints

    LoyaltyPointSummaryRetrieved = 3500,
    LoyaltyPointTransactionsRetrieved = 3501,
    LoyaltyPointsAwarded = 3502,
    MinimumRedemptionNotMet= 3503,
    InsufficientLoyaltyPoints = 3504,
    LoyaltyPointsRedeemed = 3505,
    PointsAlreadyAwarded = 3506,
    BookingPointsProcessed = 3507,
    ExpiredPointsProcessed = 3508,
    #endregion

    #region Extra Service 

    ExtrasRetrieved = 4000,
    ExtraServiceNotFound = 4001,
    ExtraRetrieved = 4002,
    #endregion

    #region Fav
    FavoritesRetrieved = 4500,
    CarAlreadyInFavorites = 4501,
    FavoriteAdded = 4502,
    FavoriteNotFound  = 4503,
    FavoriteUpdated = 4504,
    FavoriteRemoved = 4505,
    FavoriteStatusRetrieved = 4506,
    #endregion

    #region Reviews
    ReviewsRetrieved = 5000,
    UserReviewsRetrieved = 5001,
    ReviewNotFound = 5002,
    CarRatingRetrieved = 5003,
    CanReview = 5004,
    MustCompleteBookingToReview = 5005
    #endregion
}