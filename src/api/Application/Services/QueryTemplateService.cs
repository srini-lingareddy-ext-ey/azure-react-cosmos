using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Monitor = Todo.Api.Domain.Entities.Monitor;

namespace Todo.Api.Application.Services;

/// <summary>WO-46: query template CRUD with propagation to existing monitors.</summary>
public sealed class QueryTemplateService : IQueryTemplateService
{
    private readonly IQueryTemplateRepository _templateRepo;
    private readonly IMonitorRepository _monitorRepo;

    public QueryTemplateService(
        IQueryTemplateRepository templateRepo,
        IMonitorRepository monitorRepo)
    {
        _templateRepo = templateRepo;
        _monitorRepo = monitorRepo;
    }

    public async Task<QueryTemplateListResponse> ListAsync(
        string tenantId, CancellationToken cancellationToken = default)
    {
        var items = new List<QueryTemplate>();
        await foreach (var qt in _templateRepo.GetAllByTenantAsync(tenantId, cancellationToken).ConfigureAwait(false))
        {
            items.Add(qt);
        }

        return new QueryTemplateListResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = items.Count,
        };
    }

    public async Task<QueryTemplateResponse> GetByIdAsync(
        string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var qt = await _templateRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (qt is null) throw new KeyNotFoundException("Query template not found.");
        return MapToResponse(qt);
    }

    public async Task<QueryTemplateResponse> CreateAsync(
        string userId, string tenantId, CreateQueryTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new QueryTemplate
        {
            Id = Guid.NewGuid().ToString("n"),
            TenantId = tenantId,
            TemplateName = request.TemplateName.Trim(),
            ConnectorTypeId = request.ConnectorTypeId.Trim(),
            TemplateBody = request.TemplateBody,
            Parameters = request.Parameters ?? [],
            IsActive = true,
            SchemaVersion = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
        };

        var created = await _templateRepo.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        return MapToResponse(created);
    }

    public async Task<QueryTemplateResponse> UpdateAsync(
        string userId, string id, string tenantId, UpdateQueryTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var qt = await _templateRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (qt is null) throw new KeyNotFoundException("Query template not found.");

        if (request.TemplateName is not null) qt.TemplateName = request.TemplateName.Trim();
        if (request.TemplateBody is not null) qt.TemplateBody = request.TemplateBody;
        if (request.Parameters is not null) qt.Parameters = request.Parameters;

        qt.UpdatedAt = DateTimeOffset.UtcNow;
        qt.UpdatedBy = userId;

        var updated = await _templateRepo.UpdateAsync(qt, cancellationToken).ConfigureAwait(false);

        // Propagate template body to all existing monitors using this template
        if (string.Equals(request.PropagationMode, "allExisting", StringComparison.OrdinalIgnoreCase))
        {
            await foreach (var m in _monitorRepo.GetAllByTenantAsync(tenantId, cancellationToken).ConfigureAwait(false))
            {
                if (m.QueryTemplateId == id)
                {
                    m.QueryTemplateSnapshot = updated.TemplateBody;
                    m.UpdatedAt = DateTimeOffset.UtcNow;
                    m.UpdatedBy = userId;
                    await _monitorRepo.UpdateAsync(m, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        return MapToResponse(updated);
    }

    internal static QueryTemplateResponse MapToResponse(QueryTemplate qt)
    {
        return new QueryTemplateResponse
        {
            Id = qt.Id,
            TenantId = qt.TenantId,
            TemplateName = qt.TemplateName,
            ConnectorTypeId = qt.ConnectorTypeId,
            TemplateBody = qt.TemplateBody,
            Parameters = qt.Parameters,
            IsActive = qt.IsActive,
            SchemaVersion = qt.SchemaVersion,
            CreatedAt = qt.CreatedAt,
            UpdatedAt = qt.UpdatedAt,
            CreatedBy = qt.CreatedBy,
            UpdatedBy = qt.UpdatedBy,
        };
    }
}
