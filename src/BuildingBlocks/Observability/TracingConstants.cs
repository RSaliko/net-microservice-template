using System.Diagnostics;

namespace BuildingBlocks.Observability;

/// <summary>
/// BP #23: Centralized ActivitySource for Distributed Tracing.
/// </summary>
public static class TracingConstants
{
    public const string SourceName = "Microservice.BuildingBlocks";
    public static readonly ActivitySource ActivitySource = new(SourceName);
}
