using Microsoft.Extensions.Caching.Distributed;

namespace Todo.Api.Application.Services;

/// <summary>WO-66: INC-YYYYMMDD-NNN display ID via distributed cache sequence.</summary>
public sealed class DisplayIdGenerationService : IDisplayIdGenerationService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DisplayIdGenerationService> _logger;

    public DisplayIdGenerationService(IDistributedCache cache, ILogger<DisplayIdGenerationService> logger)
    { _cache = cache; _logger = logger; }

    public async Task<string> GenerateAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var key = $"incident:seq:{tenantId}:{date}";
        var raw = await _cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
        var seq = 1;
        if (int.TryParse(raw, out var current)) seq = current + 1;
        await _cache.SetStringAsync(key, seq.ToString(), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(36) }, cancellationToken).ConfigureAwait(false);
        var displayId = $"INC-{date}-{seq:D3}";
        _logger.LogDebug("Generated display ID {DisplayId} for tenant {TenantId}", displayId, tenantId);
        return displayId;
    }
}
