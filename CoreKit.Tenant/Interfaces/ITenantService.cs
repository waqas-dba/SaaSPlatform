using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Models;

namespace CoreKit.Tenant.Interfaces;

public interface ITenantService
{
    Task<TenantRegistrationResponse> RegisterAsync(TenantRegistrationRequest request);
    Task<TenantEntity?> GetByIdAsync(Guid tenantId);
    Task<List<TenantEntity>> GetAllAsync();
    Task UpdateAsync(Guid tenantId, string? name, string? metadataJson);
    Task DeleteAsync(Guid tenantId);
    Task ApproveAsync(Guid tenantId);
    Task RejectAsync(Guid tenantId);
}