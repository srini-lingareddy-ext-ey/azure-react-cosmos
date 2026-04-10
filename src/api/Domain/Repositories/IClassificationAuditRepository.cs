using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IClassificationAuditRepository
{
    IAsyncEnumerable<ClassificationAuditEntry> GetByTenantAsync(string tenantId, string? outcome, string? matchedRuleId, DateTimeOffset? from, DateTimeOffset? to, int limit = 50, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClassificationAuditEntry> CreateAsync(ClassificationAuditEntry entry, CancellationToken cancellationToken = default);
}
