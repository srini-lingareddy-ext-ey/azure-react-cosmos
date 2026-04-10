using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class ProductAvailabilityRepository : IProductAvailabilityRepository
{
    private readonly IRepository<ProductAvailability> _repository;
    public ProductAvailabilityRepository(IRepository<ProductAvailability> repository) { _repository = repository; }

    public Task<ProductAvailability?> GetByProductIdAsync(string productId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(productId, tenantId, cancellationToken);

    public IAsyncEnumerable<ProductAvailability> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<ProductAvailability> UpsertAsync(ProductAvailability entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = entity.ProductId;
        return _repository.UpsertAsync(entity, cancellationToken);
    }
}
