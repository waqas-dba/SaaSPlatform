// CoreKit.Catalog | Interfaces/IVariantAttributeTemplateService.cs
using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IVariantAttributeTemplateService
{
    // Platform
    Task<List<VariantAttributeTemplateDto>> GetByStoreTypeAsync(string storeTypeCode);
    Task<List<VariantAttributeTemplateDto>> GetPlatformTemplatesAsync(string storeTypeCode);  // <-- add this
    Task<VariantAttributeTemplateDto> CreatePlatformTemplateAsync(CreateVariantAttributeTemplateRequest request);
    Task UpdatePlatformTemplateAsync(Guid id, UpdateVariantAttributeTemplateRequest request);
    Task DeletePlatformTemplateAsync(Guid id);

    // Tenant
    Task<List<VariantAttributeTemplateDto>> GetTenantTemplatesAsync(Guid tenantId, string storeTypeCode);
    Task<VariantAttributeTemplateDto> CreateTenantTemplateAsync(Guid tenantId, CreateVariantAttributeTemplateRequest request);
    Task UpdateTenantTemplateAsync(Guid id, Guid tenantId, UpdateVariantAttributeTemplateRequest request);
    Task DeleteTenantTemplateAsync(Guid id, Guid tenantId);
}