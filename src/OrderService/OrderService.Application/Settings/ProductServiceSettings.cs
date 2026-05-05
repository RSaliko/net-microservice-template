namespace OrderService.Application.Settings;

public class ProductServiceSettings
{
    public const string SectionName = "ProductService";
    public string BaseUrl { get; init; } = "http://localhost:5170";
}
