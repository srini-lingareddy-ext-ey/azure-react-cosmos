using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

public sealed class InfraHealthEvaluationService : IInfraHealthEvaluationService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IComponentHealthStatusRepository _componentRepo;
    private readonly INodeHealthStatusRepository _nodeRepo;
    private readonly IInfraThresholdConfigRepository _configRepo;
    private readonly IProductAvailabilityRepository _productRepo;
    private readonly ILogger<InfraHealthEvaluationService> _logger;

    public InfraHealthEvaluationService(
        ITenantRepository tenantRepo,
        IComponentHealthStatusRepository componentRepo,
        INodeHealthStatusRepository nodeRepo,
        IInfraThresholdConfigRepository configRepo,
        IProductAvailabilityRepository productRepo,
        ILogger<InfraHealthEvaluationService> logger)
    {
        _tenantRepo = tenantRepo;
        _componentRepo = componentRepo;
        _nodeRepo = nodeRepo;
        _configRepo = configRepo;
        _productRepo = productRepo;
        _logger = logger;
    }

    public async Task EvaluateAllAsync(CancellationToken ct)
    {
        await foreach (var tenant in _tenantRepo.GetAllAsync(ct).ConfigureAwait(false))
        {
            await EvaluateTenantAsync(tenant.Id, ct).ConfigureAwait(false);
        }
    }

    private async Task EvaluateTenantAsync(string tenantId, CancellationToken ct)
    {
        var components = new List<ComponentHealthStatus>();
        await foreach (var c in _componentRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false))
            components.Add(c);

        foreach (var component in components)
        {
            var nodes = new List<NodeHealthStatus>();
            await foreach (var n in _nodeRepo.GetByComponentIdAsync(component.ComponentId, tenantId, ct).ConfigureAwait(false))
                nodes.Add(n);

            if (nodes.Count == 0)
            {
                component.Status = InfraHealthState.Unknown;
                component.NodeCount = 0;
                component.UnhealthyNodeCount = 0;
            }
            else
            {
                var config = await _configRepo.GetByComponentIdAsync(component.ComponentId, tenantId, ct).ConfigureAwait(false);
                var stalenessThreshold = config?.StalenessThresholdSeconds ?? 600;

                var worstStatus = InfraHealthState.Healthy;
                var unhealthy = 0;
                foreach (var node in nodes)
                {
                    if (node.LastMetricReceivedAt.HasValue &&
                        (DateTimeOffset.UtcNow - node.LastMetricReceivedAt.Value).TotalSeconds > stalenessThreshold)
                    {
                        node.IsStale = true;
                    }

                    if (node.Status > worstStatus) worstStatus = node.Status;
                    if (node.Status != InfraHealthState.Healthy) unhealthy++;
                }

                component.Status = worstStatus;
                component.NodeCount = nodes.Count;
                component.UnhealthyNodeCount = unhealthy;
                component.IsStale = nodes.Any(n => n.IsStale);
            }

            component.EvaluatedAt = DateTimeOffset.UtcNow;
            await _componentRepo.UpsertAsync(component, ct).ConfigureAwait(false);
        }
    }
}
