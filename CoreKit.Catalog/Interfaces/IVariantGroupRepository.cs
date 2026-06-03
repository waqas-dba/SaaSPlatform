using CoreKit.Catalog.Entities;

namespace CoreKit.Catalog.Interfaces;

public interface IVariantGroupRepository
{
    Task<VariantGroup?> GetByIdWithOptionsAsync(Guid id, Guid tenantId, CancellationToken ct = default);
}