namespace Todo.Api.Infrastructure.Integrations;

/// <summary>WO-66: stub ServiceNow client.</summary>
public sealed class ServiceNowClient : IServiceNowClient
{
    private readonly ILogger<ServiceNowClient> _logger;
    public ServiceNowClient(ILogger<ServiceNowClient> logger) { _logger = logger; }

    public Task<CreateTicketResult> CreateTicketAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("ServiceNow CreateTicket stub called. ShortDesc={Desc}", request.ShortDescription);
        return Task.FromResult(new CreateTicketResult("STUB-001", "https://servicenow.example.com/stub"));
    }

    public Task UpdateTicketStateAsync(string ticketNumber, string state, CancellationToken cancellationToken = default)
    { _logger.LogWarning("ServiceNow UpdateTicketState stub for {Ticket} -> {State}", ticketNumber, state); return Task.CompletedTask; }

    public Task AddWorkNoteAsync(string ticketNumber, string note, CancellationToken cancellationToken = default)
    { _logger.LogWarning("ServiceNow AddWorkNote stub for {Ticket}", ticketNumber); return Task.CompletedTask; }

    public Task<ServiceNowTicket?> GetTicketAsync(string ticketNumber, CancellationToken cancellationToken = default)
    { _logger.LogWarning("ServiceNow GetTicket stub for {Ticket}", ticketNumber); return Task.FromResult<ServiceNowTicket?>(null); }
}
