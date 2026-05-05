namespace BuildingBlocks.Common;

public static class Constants
{
    public static class Headers
    {
        public const string IdempotencyKey = "X-Idempotency-Key";
        public const string CorrelationId = "X-Correlation-Id";
    }

    public static class Cache
    {
        public const int DefaultExpirationMinutes = 60;
    }
}
