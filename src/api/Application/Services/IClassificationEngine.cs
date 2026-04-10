using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Services;

public interface IClassificationEngine
{
    Task<(EventClassification classification, string matchedRuleId)> ClassifyAsync(Event evt, CancellationToken ct);
}
