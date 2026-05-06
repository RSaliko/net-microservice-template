namespace BuildingBlocks.Common;

/// <summary>
/// BP #32: Standardized Internal Error Codes using PREFIX_ENTITY_CODE pattern.
/// </summary>
public static class ErrorCodes
{
    public static class System
    {
        public const string InternalError = "SYS_INTERNAL_ERROR";
        public const string ServiceUnavailable = "SYS_SERVICE_UNAVAILABLE";
        public const string ExternalApiError = "SYS_EXTERNAL_API_ERROR";
    }

    public static class Validation
    {
        public const string GeneralFailure = "VAL_GENERAL_FAILURE";
        public const string InvalidInput = "VAL_INVALID_INPUT";
        public const string RequiredFieldMissing = "VAL_REQUIRED_FIELD";
    }

    public static class Auth
    {
        public const string Unauthorized = "AUTH_UNAUTHORIZED";
        public const string Forbidden = "AUTH_FORBIDDEN";
        public const string InvalidToken = "AUTH_INVALID_TOKEN";
        public const string TokenExpired = "AUTH_TOKEN_EXPIRED";
    }

    public static class Order
    {
        public const string NotFound = "ORD_NOT_FOUND";
        public const string AlreadyExists = "ORD_ALREADY_EXISTS";
        public const string InvalidStatus = "ORD_INVALID_STATUS";
        public const string CreateFailed = "ORD_CREATE_FAILED";
    }

    public static class Product
    {
        public const string NotFound = "PRD_NOT_FOUND";
        public const string InsufficientStock = "PRD_INSUFFICIENT_STOCK";
        public const string DeactivationFailed = "PRD_DEACTIVATION_FAILED";
    }
}
