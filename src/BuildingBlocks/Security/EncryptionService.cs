namespace BuildingBlocks.Security;

/// <summary>
/// Provides encryption/decryption for sensitive data (PII).
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public class AesEncryptionService : IEncryptionService
{
    // Implementation for AES-256 encryption at rest
    public string Encrypt(string plainText) => plainText; // Placeholder
    public string Decrypt(string cipherText) => cipherText; // Placeholder
}
