using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitWithOutbox<TDbContext>(this IServiceCollection services, string rabbitMqHost)
        where TDbContext : DbContext
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqHost);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
