using System.Text.Json;
using Todo.Api.Application.EventPublishing;
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
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<InfraHealthEvaluationService> _logger;

    public InfraHealthEvaluationService(
        ITenantRepository tenantRepo,
        IComponentHealthStatusRepository componentRepo,
        INodeHealthStatusRepository nodeRepo,
        IInfraThresholdConfigRepository configRepo,
        IProductAvailabilityRepository productRepo,
        IEventPublisher eventPublisher,
        ILogger<InfraHealthEvaluationService> logger)
    {
        _tenantRepo = tenantRepo;
        _componentRepo = componentRepo;
        _nodeRepo = nodeRepo;
        _configRepo = configRepo;
        _productRepo = productRepo;
        _eventPublisher = eventPublisher;
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
            var previousStatus = component.Status;

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

            if (component.Status != previousStatus)
            {
                await PublishTransitionEventAsync(tenantId, component, previousStatus, ct).ConfigureAwait(false);
            }
        }

        await EvaluateProductAvailabilityAsync(tenantId, ct).ConfigureAwait(false);
    }

    private async Task PublishTransitionEventAsync(
        string tenantId, ComponentHealthStatus component, InfraHealthState previousStatus, CancellationToken ct)
    {
        var severity = component.Status == InfraHealthState.Critical ? "Critical" : "Warning";
        var payload = JsonSerializer.Serialize(new
        {
            tenantId,
            componentName = component.ComponentName,
            componentId = component.ComponentId,
            previousStatus = previousStatus.ToString(),
            currentStatus = component.Status.ToString(),
            evaluatedAt = component.EvaluatedAt,
            nodeCount = component.NodeCount,
            unhealthyNodeCount = component.UnhealthyNodeCount,
        });

        var evt = new NormalizedEvent
        {
            EventType = $"infra.threshold.{severity.ToLowerInvariant()}",
            TenantId = tenantId,
            Payload = payload,
        };

        await _eventPublisher.PublishAsync("InfrastructureEvents", evt, ct).ConfigureAwait(false);
        _logger.LogInformation(
            "Published {Severity} threshold breach for component {Component}: {Previous} -> {Current}",
            severity, component.ComponentName, previousStatus, component.Status);
    }

    private async Task EvaluateProductAvailabilityAsync(string tenantId, CancellationToken ct)
    {
        var products = new List<ProductAvailability>();
        await foreach (var p in _productRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false))
            products.Add(p);

        foreach (var product in products)
        {
            if (product.HeartbeatIntervalSeconds <= 0)
                continue;

            var stalenessThreshold = product.HeartbeatIntervalSeconds * 3;

            if (product.LastHeartbeatAt.HasValue &&
                (DateTimeOffset.UtcNow - product.LastHeartbeatAt.Value).TotalSeconds > stalenessThreshold)
            {
                product.Status = InfraHealthState.Unknown;
            }
            else if (product.LastHeartbeatAt.HasValue)
            {
                var expectedHeartbeats = 86400.0 / product.HeartbeatIntervalSeconds;
                var actual = product.HeartbeatCount24h;
                product.Availability24h = expectedHeartbeats > 0
                    ? Math.Round(actual / expectedHeartbeats * 100, 2)
                    : 0;
                product.Status = InfraHealthState.Healthy;
            }

            product.UpdatedAt = DateTimeOffset.UtcNow;
            await _productRepo.UpsertAsync(product, ct).ConfigureAwait(false);
        }
    }
}
