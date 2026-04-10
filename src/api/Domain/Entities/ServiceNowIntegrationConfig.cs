namespace Todo.Api.Domain.Entities;

public enum ServiceNowAuthType { Basic = 0, OAuth = 1 }

public sealed class ServiceNowIntegrationConfig : IDomainEntity, IConcurrencyEntity, IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public ServiceNowAuthType AuthType { get; set; } = ServiceNowAuthType.Basic;
    public string CredentialSecretName { get; set; } = string.Empty;
    public string? CallerUserId { get; set; }
    public string? TicketTemplate { get; set; }
    public Dictionary<string, int> UrgencyMapping { get; set; } = new();
    public Dictionary<string, string> SeverityMapping { get; set; } = new();
    public Dictionary<string, string> StateMapping { get; set; } = new();
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public object PartitionKeyValue => TenantId;
}