using System.Diagnostics;

namespace BuildingBlocks.Common;

public static class TelemetryConstants
{
    public const string ServiceName = "MicroserviceTemplate";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
}
