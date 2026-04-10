using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IClassificationRuleRepository
{
    IAsyncEnumerable<ClassificationRule> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<ClassificationRule?> GetByIdAsync(string ruleId, string tenantId, CancellationToken cancellationToken = default);
    Task<ClassificationRule> UpsertAsync(ClassificationRule rule, CancellationToken cancellationToken = default);
}
