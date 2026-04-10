namespace Todo.Api.Application.Services;

/// <summary>Encrypts/decrypts credential blobs with per-tenant key derivation (WO-17).</summary>
public interface ICredentialEncryptionService
{
    Task<string> EncryptAsync(string plaintext, string tenantId);
    Task<string> DecryptAsync(string ciphertext, string tenantId);
}
