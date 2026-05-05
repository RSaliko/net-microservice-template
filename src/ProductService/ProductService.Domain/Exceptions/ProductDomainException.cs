namespace ProductService.Domain.Exceptions;

public class ProductDomainException : Exception
{
    public string ErrorCode { get; }

    public ProductDomainException(string message, string errorCode = "Product_DOMAIN_ERROR") 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
