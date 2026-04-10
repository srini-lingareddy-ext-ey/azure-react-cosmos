using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-43: pipeline registration CRUD with deactivation/monitor suspension.</summary>
public interface IPipelineRegistrationService
{
    Task<PipelineRegistrationListResponse> ListAsync(string tenantId, string? businessPlanId, string? medallionLayer, CancellationToken cancellationToken = default);
    Task<PipelineRegistrationResponse> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<PipelineRegistrationResponse> CreateAsync(string userId, string tenantId, CreatePipelineRegistrationRequest request, CancellationToken cancellationToken = default);
    Task<PipelineRegistrationResponse> UpdateAsync(string userId, string id, string tenantId, UpdatePipelineRegistrationRequest request, CancellationToken cancellationToken = default);
    Task<PipelineDeactivateResponse> DeactivateAsync(string userId, string id, string tenantId, CancellationToken cancellationToken = default);
    Task<PipelineRegistrationResponse> ActivateAsync(string userId, string id, string tenantId, CancellationToken cancellationToken = default);
}
