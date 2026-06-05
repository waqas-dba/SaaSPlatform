// CoreKit.IAM | Repositories/AuditLogRepository.cs
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly IamDbContext _db;

    public AuditLogRepository(IamDbContext db) => _db = db;

    public async Task<PagedResult<AuditLog>> GetLogsAsync(
        string? entityName = null,
        string? entityId = null,
        PagedQuery? query = null,
        CancellationToken ct = default)
    {
        query ??= new PagedQuery();

        var baseQuery = _db.Set<AuditLog>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entityName))
            baseQuery = baseQuery.Where(l => l.EntityName == entityName);
        if (!string.IsNullOrWhiteSpace(entityId))
            baseQuery = baseQuery.Where(l => l.EntityId == entityId);

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(l => l.Timestamp)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return PagedResult<AuditLog>.From(items, totalCount, query.Page, query.PageSize);
    }
}