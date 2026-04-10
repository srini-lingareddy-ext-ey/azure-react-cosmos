using System.Text.Json;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class MemSQLInterfaceEventHandler : IEventHandler
{
    public string EventType => "memsql.interface";

    private readonly IMemSQLInterfaceStatusRepository _repo;
    private readonly ILogger<MemSQLInterfaceEventHandler> _logger;

    public MemSQLInterfaceEventHandler(IMemSQLInterfaceStatusRepository repo, ILogger<MemSQLInterfaceEventHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(payload);
        var tenantId = data.GetProperty("tenantId").GetString() ?? string.Empty;
        var interfaceName = data.GetProperty("interfaceName").GetString() ?? string.Empty;

        var entity = new MemSQLInterfaceStatus
        {
            Id = $"{tenantId}_{interfaceName}",
            TenantId = tenantId,
            InterfaceName = interfaceName,
            Status = data.TryGetProperty("status", out var st) ? st.GetString() ?? string.Empty : string.Empty,
            PendingRecordCount = data.TryGetProperty("pendingRecordCount", out var prc) ? prc.GetInt64() : 0,
            LastCompletedAt = data.TryGetProperty("lastCompletedAt", out var lc) ? lc.GetDateTimeOffset() : null,
            LastErrorMessage = data.TryGetProperty("lastErrorMessage", out var le) ? le.GetString() : null,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        await _repo.UpsertAsync(entity, ct).ConfigureAwait(false);
        _logger.LogDebug("Processed MemSQL interface {InterfaceName} status={Status}", interfaceName, entity.Status);
    }
}
