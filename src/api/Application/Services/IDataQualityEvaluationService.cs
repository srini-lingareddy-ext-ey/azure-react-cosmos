namespace Todo.Api.Application.Services;

public interface IDataQualityEvaluationService
{
    Task EvaluateAllAsync(CancellationToken cancellationToken);
}
