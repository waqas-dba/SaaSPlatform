using TenantKit.Entities;
using TenantKit.Models;

namespace TenantKit.Interfaces;

public interface ITenantService
{
    Task<TenantRegistrationResponse> RegisterAsync(TenantRegistrationRequest request);
    Task<Tenant?> GetByIdAsync(Guid tenantId);
    Task<List<Tenant>> GetAllAsync();
    Task UpdateAsync(Guid tenantId, string? name, string? metadataJson);
    Task DeleteAsync(Guid tenantId);
    Task ApproveAsync(Guid tenantId);
    Task RejectAsync(Guid tenantId);
}