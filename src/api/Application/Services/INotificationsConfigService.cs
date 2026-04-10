using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-71: CRUD for notification channels, routing rules, maintenance windows, delivery logs, ServiceNow config.</summary>
public interface INotificationsConfigService
{
    Task<List<NotificationChannelDto>> GetChannelsAsync(string tenantId, CancellationToken ct = default);
    Task<NotificationChannelDto> GetChannelByIdAsync(string id, string tenantId, CancellationToken ct = default);
    Task<NotificationChannelDto> CreateChannelAsync(string tenantId, string userId, CreateNotificationChannelRequest request, CancellationToken ct = default);
    Task<NotificationChannelDto> UpdateChannelAsync(string id, string tenantId, string userId, UpdateNotificationChannelRequest request, CancellationToken ct = default);
    Task DeleteChannelAsync(string id, string tenantId, CancellationToken ct = default);
    Task<List<NotificationRoutingRuleDto>> GetRoutingRulesAsync(string tenantId, CancellationToken ct = default);
    Task<NotificationRoutingRuleDto> CreateRoutingRuleAsync(string tenantId, string userId, CreateNotificationRoutingRuleRequest request, CancellationToken ct = default);
    Task<NotificationRoutingRuleDto> UpdateRoutingRuleAsync(string id, string tenantId, string userId, UpdateNotificationRoutingRuleRequest request, CancellationToken ct = default);
    Task DeleteRoutingRuleAsync(string id, string tenantId, CancellationToken ct = default);
    Task<List<MaintenanceWindowDto>> GetMaintenanceWindowsAsync(string tenantId, CancellationToken ct = default);
    Task<MaintenanceWindowDto> CreateMaintenanceWindowAsync(string tenantId, string userId, CreateMaintenanceWindowRequest request, CancellationToken ct = default);
    Task DeleteMaintenanceWindowAsync(string id, string tenantId, CancellationToken ct = default);
    Task<List<NotificationDeliveryLogDto>> GetDeliveryLogsAsync(string tenantId, string? eventId, string? status, DateTimeOffset? from, DateTimeOffset? to, int limit, CancellationToken ct = default);
    Task<ServiceNowConfigDto?> GetServiceNowConfigAsync(string tenantId, CancellationToken ct = default);
    Task<ServiceNowConfigDto> UpsertServiceNowConfigAsync(string tenantId, string userId, UpsertServiceNowConfigRequest request, CancellationToken ct = default);
}
