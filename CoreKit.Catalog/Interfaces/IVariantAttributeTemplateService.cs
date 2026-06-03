using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IVariantAttributeTemplateService
{
    Task<List<VariantAttributeTemplateDto>> GetByStoreTypeAsync(string storeTypeCode);
    Task<VariantAttributeTemplateDto> CreateAsync(CreateVariantAttributeTemplateRequest request);
    Task UpdateAsync(Guid id, UpdateVariantAttributeTemplateRequest request);
    Task DeleteAsync(Guid id);
}