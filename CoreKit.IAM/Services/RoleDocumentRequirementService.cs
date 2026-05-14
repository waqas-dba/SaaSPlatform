using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Services;

public class RoleDocumentRequirementService : IRoleDocumentRequirementService
{
    private readonly IamDbContext _db;

    public RoleDocumentRequirementService(IamDbContext db) => _db = db;

    public async Task AddRequirementAsync(Guid roleId, string documentType)
    {
        bool exists = await _db.RoleDocumentRequirements
            .AnyAsync(r => r.RoleId == roleId && r.DocumentType == documentType);
        if (!exists)
        {
            _db.RoleDocumentRequirements.Add(new RoleDocumentRequirement
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                DocumentType = documentType
            });
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveRequirementAsync(Guid roleId, string documentType)
    {
        var req = await _db.RoleDocumentRequirements
            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.DocumentType == documentType);
        if (req != null)
        {
            _db.RoleDocumentRequirements.Remove(req);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetRequiredDocumentsAsync(Guid roleId)
    {
        return await _db.RoleDocumentRequirements
            .Where(r => r.RoleId == roleId)
            .Select(r => r.DocumentType)
            .ToListAsync();
    }

    public async Task<bool> IsRequiredAsync(Guid roleId, string documentType)
    {
        return await _db.RoleDocumentRequirements
            .AnyAsync(r => r.RoleId == roleId && r.DocumentType == documentType);
    }
}