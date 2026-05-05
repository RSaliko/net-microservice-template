namespace BuildingBlocks.Exceptions;

public class BusinessException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public BusinessException(string message, string errorCode = "BUS_ERROR", int statusCode = 400) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

public class NotFoundException : BusinessException
{
    public NotFoundException(string message, string errorCode = "SYS_NOT_FOUND") 
        : base(message, errorCode, 404) { }
}
