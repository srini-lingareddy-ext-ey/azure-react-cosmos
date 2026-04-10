using System.Security.Cryptography;
using System.Text;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Todo.Api.Application.Services;

namespace Todo.Api.Infrastructure.Security;

/// <summary>AES-256-GCM encryption with HKDF per-tenant key derivation (WO-17).</summary>
public sealed class CredentialEncryptionService : ICredentialEncryptionService
{
    private readonly CredentialEncryptionOptions _options;
    private readonly ILogger<CredentialEncryptionService> _logger;
    private readonly SecretClient? _secretClient;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private byte[]? _masterKey;

    public CredentialEncryptionService(
        IOptions<CredentialEncryptionOptions> options,
        ILogger<CredentialEncryptionService> logger,
        SecretClient? secretClient = null)
    {
        _options = options.Value;
        _logger = logger;
        _secretClient = secretClient;
    }

    public async Task<string> EncryptAsync(string plaintext, string tenantId)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintext);
        var masterKey = await ResolveMasterKeyAsync().ConfigureAwait(false);
        return EncryptCore(plaintext, tenantId, masterKey);
    }

    public async Task<string> DecryptAsync(string ciphertext, string tenantId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ciphertext);
        var masterKey = await ResolveMasterKeyAsync().ConfigureAwait(false);
        return DecryptCore(ciphertext, tenantId, masterKey);
    }

    private static string EncryptCore(string plaintext, string tenantId, byte[] masterKey)
    {
        var derivedKey = HKDF.DeriveKey(HashAlgorithmName.SHA256, masterKey, 32, info: Encoding.UTF8.GetBytes(tenantId));
        try
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce);
            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[16];

            using var aes = new AesGcm(derivedKey, 16);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            var result = new byte[12 + 16 + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, 12);
            Buffer.BlockCopy(tag, 0, result, 12, 16);
            Buffer.BlockCopy(ciphertext, 0, result, 28, ciphertext.Length);
            return Convert.ToBase64String(result);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(derivedKey);
        }
    }

    private static string DecryptCore(string ciphertextBase64, string tenantId, byte[] masterKey)
    {
        var combined = Convert.FromBase64String(ciphertextBase64);
        if (combined.Length < 28)
            throw new ArgumentException("Ciphertext is too short.", nameof(ciphertextBase64));

        var derivedKey = HKDF.DeriveKey(HashAlgorithmName.SHA256, masterKey, 32, info: Encoding.UTF8.GetBytes(tenantId));
        try
        {
            var nonce = new byte[12];
            var tag = new byte[16];
            var encrypted = new byte[combined.Length - 28];
            Buffer.BlockCopy(combined, 0, nonce, 0, 12);
            Buffer.BlockCopy(combined, 12, tag, 0, 16);
            Buffer.BlockCopy(combined, 28, encrypted, 0, encrypted.Length);

            var plaintextBytes = new byte[encrypted.Length];
            using var aes = new AesGcm(derivedKey, 16);
            aes.Decrypt(nonce, encrypted, tag, plaintextBytes);
            return Encoding.UTF8.GetString(plaintextBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(derivedKey);
        }
    }

    private async Task<byte[]> ResolveMasterKeyAsync()
    {
        if (_masterKey is not null) return _masterKey;
        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_masterKey is not null) return _masterKey;
            if (_secretClient is not null && !string.IsNullOrEmpty(_options.KeyVaultKeyName))
            {
                var response = await _secretClient.GetSecretAsync(_options.KeyVaultKeyName).ConfigureAwait(false);
                _masterKey = Convert.FromBase64String(response.Value.Value);
                return _masterKey;
            }
            if (!string.IsNullOrEmpty(_options.FallbackKeyBase64))
            {
                _logger.LogWarning("Using fallback encryption key. Not suitable for production.");
                _masterKey = Convert.FromBase64String(_options.FallbackKeyBase64);
                return _masterKey;
            }
            throw new InvalidOperationException("CredentialEncryption: Neither Key Vault key nor FallbackKeyBase64 is configured.");
        }
        finally { _initLock.Release(); }
    }
}
