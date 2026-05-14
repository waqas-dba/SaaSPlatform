using Microsoft.EntityFrameworkCore;
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;

namespace CoreKit.IAM.Services;

public class UserDocumentService : IUserDocumentService
{
    private readonly IamDbContext _db;

    public UserDocumentService(IamDbContext db) => _db = db;

    public async Task AddOrUpdateAsync(Guid userId, string documentType, string imageUrl)
    {
        var existing = await _db.UserDocuments
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DocumentType == documentType);

        if (existing != null)
        {
            existing.ImageUrl = imageUrl;
            _db.UserDocuments.Update(existing);
        }
        else
        {
            _db.UserDocuments.Add(new UserDocument
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DocumentType = documentType,
                ImageUrl = imageUrl
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task<UserDocument?> GetAsync(Guid userId, string documentType)
        => await _db.UserDocuments
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DocumentType == documentType);

    public async Task<List<UserDocument>> GetByUserAsync(Guid userId)
        => await _db.UserDocuments
            .Where(d => d.UserId == userId)
            .ToListAsync();

    public async Task DeleteAsync(Guid userId, string documentType)
    {
        var doc = await GetAsync(userId, documentType);
        if (doc != null)
        {
            _db.UserDocuments.Remove(doc);
            await _db.SaveChangesAsync();
        }
    }
}