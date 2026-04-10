using Todo.Api.Application.Services;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class ClassificationEventHandler
{
    private readonly IClassificationEngine _engine;

    public ClassificationEventHandler(IClassificationEngine engine) { _engine = engine; }

    public async Task ClassifyAsync(Event evt, CancellationToken ct)
    {
        var (classification, ruleId) = await _engine.ClassifyAsync(evt, ct).ConfigureAwait(false);
        evt.Classification = classification;
        evt.ClassificationRuleId = ruleId;
        evt.ClassifiedAt = DateTimeOffset.UtcNow;
    }
}
