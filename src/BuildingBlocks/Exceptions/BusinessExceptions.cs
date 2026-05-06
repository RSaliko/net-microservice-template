namespace BuildingBlocks.Exceptions;

public class BusinessException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public BusinessException(string message, string errorCode = Common.ErrorCodes.System.InternalError, int statusCode = 400) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

public class NotFoundException : BusinessException
{
    public NotFoundException(string message, string errorCode = Common.ErrorCodes.System.InternalError) 
        : base(message, errorCode, 404) { }
}

public class ValidationException : BusinessException
{
    public IEnumerable<object>? Errors { get; }

    public ValidationException(string message, IEnumerable<object>? errors = null, string errorCode = Common.ErrorCodes.Validation.GeneralFailure) 
        : base(message, errorCode, 400)
    {
        Errors = errors;
    }
}
