using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IProductAvailabilityRepository
{
    Task<ProductAvailability?> GetByProductIdAsync(string productId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ProductAvailability> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<ProductAvailability> UpsertAsync(ProductAvailability entity, CancellationToken cancellationToken = default);
}
