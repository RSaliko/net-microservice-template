namespace BuildingBlocks.Data;

/// <summary>
/// Interface for database initializers (Migrations + Seeding).
/// </summary>
public interface IDbInitializer
{
    /// <summary>
    /// Executes the migration and seeding logic.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Seeds the database with initial/development data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SeedAsync();
}
