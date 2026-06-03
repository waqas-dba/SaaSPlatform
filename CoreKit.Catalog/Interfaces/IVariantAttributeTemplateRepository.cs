using CoreKit.Catalog.Entities;

namespace CoreKit.Catalog.Interfaces;

public interface IVariantAttributeTemplateRepository
{
    Task<VariantAttributeTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<VariantAttributeTemplate?> GetByNameAndStoreTypeAsync(string name, string storeTypeCode, CancellationToken ct = default);
}