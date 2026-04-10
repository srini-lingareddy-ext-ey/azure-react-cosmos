namespace Todo.Api.Infrastructure.Integrations;

/// <summary>WO-66: ServiceNow REST API client abstraction.</summary>
public interface IServiceNowClient
{
    Task<CreateTicketResult> CreateTicketAsync(CreateTicketRequest request, CancellationToken cancellationToken = default);
    Task UpdateTicketStateAsync(string ticketNumber, string state, CancellationToken cancellationToken = default);
    Task AddWorkNoteAsync(string ticketNumber, string note, CancellationToken cancellationToken = default);
    Task<ServiceNowTicket?> GetTicketAsync(string ticketNumber, CancellationToken cancellationToken = default);
}

public sealed record CreateTicketRequest(string EndpointUrl, string CredentialSecretName, string ShortDescription, string Description, int Urgency, string Severity, string? CallerId);
public sealed record CreateTicketResult(string TicketNumber, string TicketUrl);
public sealed record ServiceNowTicket(string TicketNumber, string State, string? AssignedTo, DateTimeOffset? UpdatedAt);
