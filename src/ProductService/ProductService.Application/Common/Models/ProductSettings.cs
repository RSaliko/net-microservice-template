namespace ProductService.Application.Common.Models;

public class ProductSettings
{
    public const string SectionName = "ProductSettings";

    public int MaxActiveProducts { get; set; } = 10;
    public bool AutoComplete { get; set; } = false;
    public string DefaultPrefix { get; set; } = "Product-";
}
