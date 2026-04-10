using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Connectors;

/// <summary>WO-47: in-memory catalog of 13 certified connector types.</summary>
public sealed class ConnectorTypeCatalog
{
    private readonly Dictionary<string, ConnectorTypeCatalogEntry> _entries;

    public ConnectorTypeCatalog()
    {
        var list = new ConnectorTypeCatalogEntry[]
        {
            new("airflow",          "Apache Airflow",   IntegrationMode.Polling, "Certified", new[] { "host", "username", "password" }),
            new("talend",           "Talend",           IntegrationMode.Polling, "Certified", new[] { "host", "apiKey" }),
            new("hvr",              "HVR",              IntegrationMode.Polling, "Certified", new[] { "host", "username", "password" }),
            new("memsql",           "MemSQL / SingleStore", IntegrationMode.Polling, "Certified", new[] { "host", "port", "username", "password", "database" }),
            new("servicenow",       "ServiceNow",       IntegrationMode.Polling, "Certified", new[] { "instanceUrl", "username", "password" }),
            new("datadog",          "Datadog",          IntegrationMode.Polling, "Certified", new[] { "apiKey", "appKey", "site" }),
            new("newrelic",         "New Relic",        IntegrationMode.Polling, "Certified", new[] { "apiKey", "accountId" }),
            new("dynatrace",        "Dynatrace",        IntegrationMode.Polling, "Certified", new[] { "environmentUrl", "apiToken" }),
            new("postgresql",       "PostgreSQL",       IntegrationMode.Polling, "Certified", new[] { "host", "port", "username", "password", "database" }),
            new("oracle",           "Oracle Database",  IntegrationMode.Polling, "Certified", new[] { "host", "port", "username", "password", "serviceName" }),
            new("sqlserver",        "SQL Server",       IntegrationMode.Polling, "Certified", new[] { "host", "port", "username", "password", "database" }),
            new("azure-synapse",    "Azure Synapse",    IntegrationMode.Polling, "Certified", new[] { "workspaceUrl", "clientId", "clientSecret", "tenantId" }),
            new("custom-webhook",   "Custom Webhook",   IntegrationMode.Push,    "Certified", new[] { "webhookSecret" }),
        };
        _entries = list.ToDictionary(e => e.ConnectorTypeId, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ConnectorTypeCatalogEntry> GetAll() => _entries.Values.ToList();

    public ConnectorTypeCatalogEntry? GetById(string connectorTypeId) =>
        _entries.TryGetValue(connectorTypeId, out var entry) ? entry : null;
}

public sealed record ConnectorTypeCatalogEntry(
    string ConnectorTypeId,
    string DisplayName,
    IntegrationMode IntegrationMode,
    string CertificationStatus,
    string[] RequiredCredentialFields);
