using System.Diagnostics;
using BuildingBlocks.Observability;
using BuildingBlocks.Caching;
using MediatR;
using OrderService.Application.Clients;
using OrderService.Application.Features.Orders.DTOs;

namespace OrderService.Application.Features.Orders.Queries.GetOrderSummary;

/// <summary>Query: fetch OrderService Order count + live product list from ProductService via Refit.</summary>
public record GetOrderSummaryQuery : IRequest<OrderSummaryDto>;

public class GetOrderSummaryQueryHandler(IProductServiceClient productServiceClient, ICacheService cacheService) 
    : IRequestHandler<GetOrderSummaryQuery, OrderSummaryDto>
{
    public async Task<OrderSummaryDto> Handle(GetOrderSummaryQuery request, CancellationToken cancellationToken)
    {
        using var activity = TracingConstants.ActivitySource.StartActivity("GetOrderSummary");
        activity?.SetTag("order.source", "OrderService");

        const string cacheKey = "products_summary";
        var products = await cacheService.GetAsync<List<ExternalProductDto>>(cacheKey, cancellationToken);

        if (products == null)
        {
            var response = await productServiceClient.GetProductsAsync();
            products = response.Data ?? [];
            
            await cacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(1), cancellationToken);
            activity?.AddEvent(new ActivityEvent("CacheMiss_FetchedFromProductService"));
        }
        else
        {
            activity?.AddEvent(new ActivityEvent("CacheHit"));
        }

        return new OrderSummaryDto(
            TotalProducts: products.Count,
            RecentProducts: products.Take(3).ToList()
        );
    }
}
