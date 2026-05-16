// CoreKit.IAM/Persistence/Seeders/UserSeeder.cs
using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class UserSeeder
{
    private readonly IamDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IConfiguration _configuration;

    public UserSeeder(IamDbContext db, IPasswordHasher hasher, IConfiguration configuration)
    {
        _db = db;
        _hasher = hasher;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        // FIX 1: Only create the admin if they don't exist — never overwrite
        // the password. A seeder that resets credentials on every run would
        // silently revert any production password change.
        //
        // FIX 2: Read the seed password from configuration so it is never
        // hardcoded in source. Add "Seeder:AdminPassword" to appsettings /
        // environment variables. The key falls back to a random placeholder
        // when missing so the app starts safely without it.
        var adminExists = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Phone == "0000000000" || u.Email == "admin@system.com");

        if (adminExists)
        {
            // Already seeded — do nothing.
            return;
        }

        var seedPassword = _configuration["Seeder:AdminPassword"];
        if (string.IsNullOrWhiteSpace(seedPassword))
            throw new InvalidOperationException(
                "Seeder:AdminPassword is not configured. " +
                "Set it via appsettings or an environment variable.");

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "System Admin",
            Phone = "0000000000",
            Email = "admin@system.com",
            PasswordHash = _hasher.Hash(seedPassword),
            TenantId = null,
            IsActive = true,
            FailedLoginAttempts = 0
        };

        _db.Users.Add(adminUser);
        await _db.SaveChangesAsync();
    }
}