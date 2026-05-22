using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;

namespace CoreKit.Tenant.Services;

public class StoreTypeService : IStoreTypeService
{
    private readonly IStoreTypeRepository _repository;

    public StoreTypeService(IStoreTypeRepository repository) => _repository = repository;

    public async Task<List<StoreTypeDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var types = await _repository.GetAllActiveAsync(cancellationToken);

        return types.Select(t => new StoreTypeDto
        {
            Id = t.Id,
            Name = t.Name,
            Code = t.Code,
            Category = t.Category
        }).ToList();
    }
}