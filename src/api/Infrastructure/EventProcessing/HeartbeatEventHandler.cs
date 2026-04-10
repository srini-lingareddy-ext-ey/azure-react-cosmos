using System.Text.Json;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class HeartbeatEventHandler : IEventHandler
{
    public string EventType => "product.heartbeat";

    private readonly IProductAvailabilityRepository _productRepo;
    private readonly ILogger<HeartbeatEventHandler> _logger;

    public HeartbeatEventHandler(IProductAvailabilityRepository productRepo, ILogger<HeartbeatEventHandler> logger)
    {
        _productRepo = productRepo;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(payload);
        var tenantId = data.GetProperty("tenantId").GetString() ?? string.Empty;
        var productId = data.GetProperty("productId").GetString() ?? string.Empty;

        var product = await _productRepo.GetByProductIdAsync(productId, tenantId, ct).ConfigureAwait(false)
            ?? new ProductAvailability { Id = productId, TenantId = tenantId, ProductId = productId };

        product.ProductName = data.TryGetProperty("productName", out var pn) ? pn.GetString() ?? productId : productId;
        var eventTimestamp = data.TryGetProperty("timestamp", out var ts) ? ts.GetDateTimeOffset() : DateTimeOffset.UtcNow;
        product.LastHeartbeatAt = eventTimestamp;

        var windowStart = product.HeartbeatWindowResetAt ?? DateTimeOffset.UtcNow.AddHours(-24);
        if ((DateTimeOffset.UtcNow - windowStart).TotalHours >= 24)
        {
            product.HeartbeatCount24h = 1;
            product.HeartbeatWindowResetAt = DateTimeOffset.UtcNow;
        }
        else
        {
            product.HeartbeatCount24h++;
        }

        product.UpdatedAt = DateTimeOffset.UtcNow;
        await _productRepo.UpsertAsync(product, ct).ConfigureAwait(false);
        _logger.LogDebug("Processed heartbeat for product {ProductId}, count={Count}", productId, product.HeartbeatCount24h);
    }
}
