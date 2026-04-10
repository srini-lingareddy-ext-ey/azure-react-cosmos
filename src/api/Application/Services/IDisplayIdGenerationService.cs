namespace Todo.Api.Application.Services;

/// <summary>WO-66: generates human-readable incident display IDs.</summary>
public interface IDisplayIdGenerationService
{
    Task<string> GenerateAsync(string tenantId, CancellationToken cancellationToken = default);
}
