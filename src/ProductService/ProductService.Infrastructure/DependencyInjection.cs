using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Common.Interfaces;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }
}
