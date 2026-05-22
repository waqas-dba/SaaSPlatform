using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoreKit.IAM.Models;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class IamBootstrap
{
    private readonly IamDbContext _db;
    private readonly PermissionSeeder _permissionSeeder;
    private readonly RoleSeeder _roleSeeder;
    private readonly UserSeeder _userSeeder;
    private readonly RolePermissionSeeder _rolePermissionSeeder;
    private readonly UserRoleSeeder _userRoleSeeder;
    private readonly IamOptions _options;
    private readonly ILogger<IamBootstrap> _logger;

    public IamBootstrap(
        IamDbContext db,
        PermissionSeeder permissionSeeder,
        RoleSeeder roleSeeder,
        UserSeeder userSeeder,
        RolePermissionSeeder rolePermissionSeeder,
        UserRoleSeeder userRoleSeeder,
        IamOptions options,
        ILogger<IamBootstrap> logger)
    {
        _db = db;
        _permissionSeeder = permissionSeeder;
        _roleSeeder = roleSeeder;
        _userSeeder = userSeeder;
        _rolePermissionSeeder = rolePermissionSeeder;
        _userRoleSeeder = userRoleSeeder;
        _options = options;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        if (!await _db.Database.CanConnectAsync())
        {
            _logger.LogCritical("Cannot connect to database.");
            throw new InvalidOperationException("Database connection failed.");
        }

        if (_options.RunMigrationsOnBootstrap)
        {
            var pendingMigrations = await _db.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying {Count} pending migration(s)...", pendingMigrations.Count());
                await _db.Database.MigrateAsync();
            }
            else
            {
                _logger.LogInformation("No pending migrations.");
            }
        }
        else
        {
            _logger.LogInformation("RunMigrationsOnBootstrap is disabled — skipping migration check.");
        }

        _logger.LogInformation("Starting IAM bootstrap...");

        await _permissionSeeder.SeedAsync();
        await _roleSeeder.SeedAsync();
        await _userSeeder.SeedAsync();
        await _rolePermissionSeeder.SeedAsync();
        await _userRoleSeeder.SeedAsync();

        _logger.LogInformation("IAM bootstrap completed successfully.");
    }
}