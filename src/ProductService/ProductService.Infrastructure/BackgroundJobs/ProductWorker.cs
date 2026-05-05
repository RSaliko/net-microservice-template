using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductService.Application.Common.Interfaces;

namespace ProductService.Infrastructure.BackgroundProducts;

public class ProductWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductWorker> _logger;

    public ProductWorker(IServiceProvider serviceProvider, ILogger<ProductWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("ProductWorker running at: {time}", DateTimeOffset.Now);

            using (var scope = _serviceProvider.CreateScope()) // Manual scope management for background tasks
            {
                var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                // Perform background work
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
