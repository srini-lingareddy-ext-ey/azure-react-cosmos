using Microsoft.Extensions.Caching.Distributed;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

public sealed class RuleDeploymentService : IRuleDeploymentService
{
    private readonly IClassificationRuleRepository _ruleRepo;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RuleDeploymentService> _logger;

    public RuleDeploymentService(IClassificationRuleRepository ruleRepo, IDistributedCache cache, ILogger<RuleDeploymentService> logger)
    {
        _ruleRepo = ruleRepo;
        _cache = cache;
        _logger = logger;
    }

    public async Task DeployAsync(string tenantId, CancellationToken ct)
    {
        _logger.LogInformation("Deploying classification rules for tenant {TenantId}", tenantId);
        await InvalidateCacheAsync(tenantId, ct).ConfigureAwait(false);
        _logger.LogInformation("Classification rules deployed for tenant {TenantId}", tenantId);
    }

    public async Task InvalidateCacheAsync(string tenantId, CancellationToken ct)
    {
        var cacheKey = $"classification-rules:{tenantId}";
        await _cache.RemoveAsync(cacheKey, ct).ConfigureAwait(false);
        _logger.LogInformation("Classification rule cache invalidated for tenant {TenantId}", tenantId);
    }
}
