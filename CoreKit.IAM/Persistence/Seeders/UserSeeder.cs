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
            // Generate a secure random password but NEVER log it.
            // The operator must retrieve it from the database or reset it via a
            // secure out-of-band mechanism before going to production.
            seedPassword = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..16] + "A1!";

            _logger.LogWarning(
                "Seeder:AdminPassword is not configured. A random password was generated " +
                "for the admin account. Set 'Seeder:AdminPassword' in your configuration " +
                "before running in production. The password has NOT been logged.");

            // Write only to stdout (not to the structured logger / log aggregator)
            // so it appears in the local console during development but is not
            // shipped to any log sink.
            Console.WriteLine(
                $"[SEEDER] Temporary admin password (not logged): {seedPassword}");
            Console.WriteLine(
                "[SEEDER] Store this safely and rotate it before going to production.");
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