namespace CoreKit.IAM.Interfaces;

public interface IUserStoreAssignmentRepository
{
    Task<List<Guid>> GetStoreIdsByUserAsync(
        Guid userId, Guid tenantId, CancellationToken ct = default);
}