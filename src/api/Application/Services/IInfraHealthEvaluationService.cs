namespace Todo.Api.Application.Services;

public interface IInfraHealthEvaluationService
{
    Task EvaluateAllAsync(CancellationToken cancellationToken);
}
