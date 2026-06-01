// CoreKit.Catalog/Interfaces/IProductAttributeGroupService.cs
using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IProductAttributeGroupService
{
    Task<ProductAttributeGroupDto> CreateAsync(CreateAttributeGroupRequest request);
    Task UpdateAsync(Guid groupId, string newName, int sortOrder);
    Task DeleteAsync(Guid groupId);
    Task<List<ProductAttributeGroupDto>> GetByStoreTypeAsync(
        string storeTypeCode,
        Guid? tenantId = null);
}