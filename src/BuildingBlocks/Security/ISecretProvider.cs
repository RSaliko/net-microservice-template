using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Security;

/// <summary>
/// BP #33: Environment & Secret Management abstraction.
/// Enables easy swapping between IConfiguration (Local/Env) and Secret Managers (Key Vault/AWS).
/// </summary>
public interface ISecretProvider
{
    string GetSecret(string key);
    Task<string> GetSecretAsync(string key);
}

public class ConfigurationSecretProvider(IConfiguration configuration) : ISecretProvider
{
    public string GetSecret(string key)
    {
        var secret = configuration[key];
        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException($"Secret '{key}' not found in configuration.");
        
        return secret;
    }

    public Task<string> GetSecretAsync(string key)
    {
        return Task.FromResult(GetSecret(key));
    }
}
