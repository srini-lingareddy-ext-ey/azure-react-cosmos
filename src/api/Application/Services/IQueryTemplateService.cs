using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-46: query template CRUD with propagation to monitors.</summary>
public interface IQueryTemplateService
{
    Task<QueryTemplateListResponse> ListAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<QueryTemplateResponse> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<QueryTemplateResponse> CreateAsync(string userId, string tenantId, CreateQueryTemplateRequest request, CancellationToken cancellationToken = default);
    Task<QueryTemplateResponse> UpdateAsync(string userId, string id, string tenantId, UpdateQueryTemplateRequest request, CancellationToken cancellationToken = default);
}
