namespace ProductService.Domain.ValueObjects;

public record ProductPriority
{
    public int Level { get; init; }
    public string Name { get; init; }

    public static ProductPriority Low => new(1, "Low");
    public static ProductPriority Medium => new(2, "Medium");
    public static ProductPriority High => new(3, "High");
    public static ProductPriority Urgent => new(4, "Urgent");

    private ProductPriority(int level, string name)
    {
        Level = level;
        Name = name;
    }

    public static ProductPriority From(int level) => level switch
    {
        1 => Low,
        2 => Medium,
        3 => High,
        4 => Urgent,
        _ => throw new ArgumentException("Invalid priority level", nameof(level))
    };
}
