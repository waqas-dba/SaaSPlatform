using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class UserSeeder
{
    private readonly IamDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserSeeder> _logger;

    public UserSeeder(
        IamDbContext db,
        IPasswordHasher hasher,
        IConfiguration configuration,
        ILogger<UserSeeder> logger)
    {
        _db = db;
        _hasher = hasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var adminExists = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Phone == "0000000000" || u.Email == "admin@system.com");

        if (adminExists) return;

        var seedPassword = _configuration["Seeder:AdminPassword"];

        if (string.IsNullOrWhiteSpace(seedPassword))
        {
            // CRITICAL FIX — never write credentials to any output stream.
            // In production set Seeder:AdminPassword via an environment variable
            // or a secrets manager (e.g. AWS Secrets Manager, Azure Key Vault,
            // dotnet user-secrets for development).
            seedPassword = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..16] + "A1!";

            _logger.LogWarning(
                "Seeder:AdminPassword is not configured. " +
                "A random password was generated for the admin account. " +
                "Set 'Seeder:AdminPassword' in your secrets store before running in production. " +
                "The password has NOT been logged or written to any output stream. " +
                "Rotate it immediately after first login.");

            // Intentionally NOT printing the password anywhere.
            // Retrieve it by connecting to the DB directly after first boot,
            // then rotate it through the admin UI or a password-reset flow.
        }

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

        _logger.LogInformation("System admin user seeded successfully.");
    }
}