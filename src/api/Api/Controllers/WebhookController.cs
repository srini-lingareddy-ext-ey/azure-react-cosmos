using Microsoft.AspNetCore.Mvc;
using Todo.Api.Application.Connectors;
using Todo.Api.Application.Services;
using Todo.Api.Domain.Repositories;
using Todo.Api.Application.EventPublishing;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-48: push webhook endpoint for push-mode connectors.</summary>
[ApiController]
[Route("api/v1/connectors/push")]
public sealed class WebhookController : ControllerBase
{
    private readonly IConnectorInstanceRepository _connectorRepo;
    private readonly ICredentialEncryptionService _encryptionService;
    private readonly IEventPublisher _publisher;
    private readonly IEnumerable<IConnectorAdapter> _adapters;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IConnectorInstanceRepository connectorRepo,
        ICredentialEncryptionService encryptionService,
        IEventPublisher publisher,
        IEnumerable<IConnectorAdapter> adapters,
        ILogger<WebhookController> logger)
    {
        _connectorRepo = connectorRepo;
        _encryptionService = encryptionService;
        _publisher = publisher;
        _adapters = adapters;
        _logger = logger;
    }

    [HttpPost("{connectorId}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReceiveAsync(
        [FromRoute] string connectorId,
        CancellationToken cancellationToken = default)
    {
        // Read raw body
        using var reader = new StreamReader(Request.Body);
        var rawPayload = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        // Look up connector across all tenants (webhook has no tenant header)
        // MVP: connector ID is globally unique
        var connector = await FindConnectorAsync(connectorId, cancellationToken).ConfigureAwait(false);
        if (connector is null)
            return NotFound();

        var adapter = _adapters.FirstOrDefault(a =>
            string.Equals(a.ConnectorTypeId, connector.ConnectorTypeId, StringComparison.OrdinalIgnoreCase));
        if (adapter is null)
        {
            _logger.LogWarning("No adapter for push connector type {Type}", connector.ConnectorTypeId);
            return NotFound();
        }

        var evt = adapter.NormalizeEvent(rawPayload, connectorId, connector.TenantId);
        await _publisher.PublishAsync("pipeline-events", evt, cancellationToken).ConfigureAwait(false);

        return Accepted();
    }

    private async Task<Domain.Entities.ConnectorInstance?> FindConnectorAsync(string connectorId, CancellationToken cancellationToken)
    {
        // MVP: try empty tenantId (the repo uses point-read by id + partition key)
        // In production, push connectors would have a lookup index
        return await _connectorRepo.GetByIdAsync(connectorId, string.Empty, cancellationToken).ConfigureAwait(false);
    }
}
