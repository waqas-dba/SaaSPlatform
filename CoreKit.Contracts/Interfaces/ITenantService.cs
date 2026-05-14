using CoreKit.Contracts.Models;
using CoreKit.SharedKernel.Results;

namespace CoreKit.Contracts.Interfaces;

public interface ITenantService
{
    Task<Result<TenantRegistrationResponse>> RegisterAsync(TenantRegistrationRequest request);
    Task<TenantDto?> GetByIdAsync(Guid tenantId);
    Task<List<TenantDto>> GetAllAsync();
    Task UpdateAsync(Guid tenantId, string? name, string? metadataJson);
    Task DeleteAsync(Guid tenantId);
    Task ApproveAsync(Guid tenantId);
    Task RejectAsync(Guid tenantId);
}