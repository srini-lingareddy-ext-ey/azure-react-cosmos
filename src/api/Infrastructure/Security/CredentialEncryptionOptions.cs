namespace Todo.Api.Infrastructure.Security;

public sealed class CredentialEncryptionOptions
{
    public const string SectionName = "CredentialEncryption";
    public string? KeyVaultKeyName { get; set; }
    public string? FallbackKeyBase64 { get; set; }
}
