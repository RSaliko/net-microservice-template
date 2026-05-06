using BuildingBlocks.Security;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Data;

/// <summary>
/// EF Core Value Converter for transparent PII encryption.
/// </summary>
public class EncryptedValueConverter(IEncryptionService encryptionService, ConverterMappingHints? mappingHints = null)
    : ValueConverter<string, string>(
        v => encryptionService.Encrypt(v),
        v => encryptionService.Decrypt(v),
        mappingHints)
{
}
