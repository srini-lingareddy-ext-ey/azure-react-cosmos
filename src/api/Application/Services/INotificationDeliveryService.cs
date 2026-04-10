namespace Todo.Api.Application.Services;

/// <summary>WO-69: Notification delivery orchestration.</summary>
public interface INotificationDeliveryService
{
    Task DeliverAsync(string eventId, string tenantId, string monitorId, string businessPlan, string classification, string severity, CancellationToken cancellationToken = default);
    Task DeliverEscalationAsync(string incidentId, string tenantId, CancellationToken cancellationToken = default);
}
