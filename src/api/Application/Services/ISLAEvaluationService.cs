namespace Todo.Api.Application.Services;

public interface ISLAEvaluationService
{
    Task EvaluateAllAsync(CancellationToken cancellationToken);
}
