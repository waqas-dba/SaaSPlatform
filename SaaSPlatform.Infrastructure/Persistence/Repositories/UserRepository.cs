using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.IAM.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SaaSPlatformDbContext _db;
    public UserRepository(SaaSPlatformDbContext db) => _db = db;
    public void Add(User user) => _db.Users.Add(user);
}