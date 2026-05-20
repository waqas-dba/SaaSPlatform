using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Repositories;

public class UserStoreAssignmentRepository : IUserStoreAssignmentRepository
{
    private readonly IamDbContext _db;

    public UserStoreAssignmentRepository(IamDbContext db) => _db = db;

    public async Task<List<Guid>> GetStoreIdsByUserAsync(
        Guid userId, Guid tenantId, CancellationToken ct = default)
        => await _db.Set<UserStoreAssignment>()
            .Where(a => a.UserId == userId && a.TenantId == tenantId)
            .Select(a => a.StoreId)
            .ToListAsync(ct);
}