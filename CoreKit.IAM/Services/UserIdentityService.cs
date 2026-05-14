using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Services;

public class UserIdentityService : IUserIdentityService
{
    private readonly IamDbContext _db;
    private readonly IEncryptionService _crypto;

    public UserIdentityService(IamDbContext db, IEncryptionService crypto)
    {
        _db = db;
        _crypto = crypto;
    }

    public async Task SetCnicAsync(Guid userId, string plainCnic)
    {
        var encrypted = _crypto.Encrypt(plainCnic);

        var existing = await _db.UserIdentities
            .FirstOrDefaultAsync(i => i.UserId == userId);

        if (existing != null)
        {
            existing.EncryptedCnic = encrypted;
            _db.UserIdentities.Update(existing);
        }
        else
        {
            _db.UserIdentities.Add(new UserIdentity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EncryptedCnic = encrypted
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task<string?> GetCnicAsync(Guid userId)
    {
        var identity = await _db.UserIdentities
            .FirstOrDefaultAsync(i => i.UserId == userId);
        return identity == null ? null : _crypto.Decrypt(identity.EncryptedCnic);
    }

    public async Task DeleteAsync(Guid userId)
    {
        var identity = await _db.UserIdentities
            .FirstOrDefaultAsync(i => i.UserId == userId);
        if (identity != null)
        {
            _db.UserIdentities.Remove(identity);
            await _db.SaveChangesAsync();
        }
    }
}