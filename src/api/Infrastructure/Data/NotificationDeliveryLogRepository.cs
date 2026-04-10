using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed notification delivery log repository (WO-65). Partition key /tenantId.</summary>
public sealed class NotificationDeliveryLogRepository : INotificationDeliveryLogRepository
{
    private readonly IRepository<NotificationDeliveryLog> _repository;
    public NotificationDeliveryLogRepository(IRepository<NotificationDeliveryLog> repository) { _repository = repository; }

    public IAsyncEnumerable<NotificationDeliveryLog> GetByTenantAsync(string tenantId, string? status, DateTimeOffset? from, DateTimeOffset? to, int limit = 50, int offset = 0, CancellationToken ct = default)
    {
        var conditions = new List<string> { "c.tenantId = @tenantId" };
        var parameters = new Dictionary<string, object> { ["@tenantId"] = tenantId };
        if (!string.IsNullOrEmpty(status)) { conditions.Add("c.deliveryStatus = @status"); parameters["@status"] = Enum.Parse<DeliveryStatus>(status, true); }
        if (from.HasValue) { conditions.Add("c.sentAt >= @from"); parameters["@from"] = from.Value; }
        if (to.HasValue) { conditions.Add("c.sentAt <= @to"); parameters["@to"] = to.Value; }
        var where = string.Join(" AND ", conditions);
        var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.sentAt DESC OFFSET @offset LIMIT @limit";
        parameters["@offset"] = offset;
        parameters["@limit"] = limit;
        return _repository.QueryAsync(new QuerySpec(sql, parameters), ct);
    }

    public Task<NotificationDeliveryLog> CreateAsync(NotificationDeliveryLog log, CancellationToken ct = default) =>
        _repository.CreateAsync(log, ct);
}
