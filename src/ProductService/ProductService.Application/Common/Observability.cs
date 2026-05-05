using System.Diagnostics;

namespace ProductService.Application.Common;

public static class Observability
{
    public static readonly ActivitySource ActivitySource = new("ProductService.Application");
}
