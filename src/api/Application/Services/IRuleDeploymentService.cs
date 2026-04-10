namespace Todo.Api.Application.Services;

public interface IRuleDeploymentService
{
    Task DeployAsync(string tenantId, CancellationToken ct);
    Task InvalidateCacheAsync(string tenantId, CancellationToken ct);
}
