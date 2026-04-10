using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class ClassificationRuleRepository : IClassificationRuleRepository
{
    private readonly IRepository<ClassificationRule> _repository;
    public ClassificationRuleRepository(IRepository<ClassificationRule> repository) { _repository = repository; }

    public IAsyncEnumerable<ClassificationRule> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId AND c.isActive = true ORDER BY c.priority ASC",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<ClassificationRule?> GetByIdAsync(string ruleId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(ruleId, tenantId, cancellationToken);

    public Task<ClassificationRule> UpsertAsync(ClassificationRule rule, CancellationToken cancellationToken = default) =>
        _repository.UpsertAsync(rule, cancellationToken);
}
