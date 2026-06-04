namespace CoreKit.IAM.Interfaces;

public interface IUserStoreAssignmentService
{
    Task<List<Guid>> GetStoresForUserAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
    Task AssignUserToStoreAsync(Guid userId, Guid storeId, Guid tenantId, CancellationToken ct = default);
    Task UnassignUserFromStoreAsync(Guid userId, Guid storeId, Guid tenantId, CancellationToken ct = default);
}