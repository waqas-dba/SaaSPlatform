using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Services;

public class UserStoreAssignmentService : IUserStoreAssignmentService
{
    private readonly IamDbContext _db;

    public UserStoreAssignmentService(IamDbContext db) => _db = db;

    public async Task<List<Guid>> GetStoresForUserAsync(Guid userId, Guid tenantId, CancellationToken ct)
    {
        return await _db.UserStoreAssignments
            .Where(a => a.UserId == userId && a.TenantId == tenantId)
            .Select(a => a.StoreId)
            .ToListAsync(ct);
    }

    public async Task AssignUserToStoreAsync(Guid userId, Guid storeId, Guid tenantId, CancellationToken ct)
    {
        var exists = await _db.UserStoreAssignments
            .AnyAsync(a => a.UserId == userId && a.StoreId == storeId && a.TenantId == tenantId, ct);
        if (!exists)
        {
            _db.UserStoreAssignments.Add(new UserStoreAssignment
            {
                UserId = userId,
                StoreId = storeId,
                TenantId = tenantId
            });
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task UnassignUserFromStoreAsync(Guid userId, Guid storeId, Guid tenantId, CancellationToken ct)
    {
        var assignment = await _db.UserStoreAssignments
            .FirstOrDefaultAsync(a => a.UserId == userId && a.StoreId == storeId && a.TenantId == tenantId, ct);
        if (assignment != null)
        {
            _db.UserStoreAssignments.Remove(assignment);
            await _db.SaveChangesAsync(ct);
        }
    }
}