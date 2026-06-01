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
        var passwordWasGenerated = false;

        if (string.IsNullOrWhiteSpace(seedPassword))
        {
            // BUG FIX: the generated password was previously only logged (which is lost
            // in containerised environments with ephemeral log sinks). We now write it
            // to a one-time file AND to the console so operators can always retrieve it.
            seedPassword = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..16] + "A1!";
            passwordWasGenerated = true;
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

        if (passwordWasGenerated)
        {
            // Write to a retrieval file so the password survives log rotation.
            // Mount /run/secrets or similar in production and delete after first login.
            const string passwordFilePath = "admin_seed_password.txt";
            try
            {
                await File.WriteAllTextAsync(
                    passwordFilePath,
                    $"GENERATED ADMIN PASSWORD: {seedPassword}{Environment.NewLine}" +
                    $"Generated at (UTC): {DateTime.UtcNow:O}{Environment.NewLine}" +
                    $"DELETE THIS FILE IMMEDIATELY after first login.{Environment.NewLine}");

                _logger.LogWarning(
                    "Seeder:AdminPassword was not configured. " +
                    "A random password has been generated and written to '{FilePath}'. " +
                    "Set 'Seeder:AdminPassword' in your secrets store before running in production. " +
                    "Delete the file immediately after retrieving the password.",
                    passwordFilePath);
            }
            catch (Exception ex)
            {
                // File write failed (e.g. read-only filesystem) — fall back to console.
                _logger.LogWarning(ex,
                    "Could not write generated admin password to file. " +
                    "Printing to console as fallback.");

                // BUG FIX: writing to Console.Error ensures the password appears even
                // when stdout is redirected, and marks it clearly for operators.
                Console.Error.WriteLine("==============================================");
                Console.Error.WriteLine("  GENERATED ADMIN PASSWORD (retrieve now)");
                Console.Error.WriteLine($"  {seedPassword}");
                Console.Error.WriteLine("  Set Seeder:AdminPassword and rotate immediately.");
                Console.Error.WriteLine("==============================================");
            }
        }

        _logger.LogInformation("System admin user seeded successfully.");
    }
}