// CoreKit.IAM/Persistence/Seeders/PermissionSeeder.cs
using CoreKit.IAM.Constants;
using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class PermissionSeeder
{
    private readonly IamDbContext _db;

    // OCP: derive module code from permission name prefix automatically
    // No hardcoded map — any new "module.action" permission
    // creates its own module entry without touching this class
    private static readonly IReadOnlyDictionary<string, (Guid Id, string Name)>
        WellKnownModules = new Dictionary<string, (Guid, string)>
        {
            ["iam"] = (Guid.Parse("30000000-0000-0000-0000-000000000004"), "IAM"),
            ["catalog"] = (Guid.Parse("30000000-0000-0000-0000-000000000005"), "Catalog"),
            ["store"] = (Guid.Parse("30000000-0000-0000-0000-000000000006"), "Store"),
            ["platform"] = (Guid.Parse("30000000-0000-0000-0000-000000000007"), "Platform"),
            ["users"] = (Guid.Parse("30000000-0000-0000-0000-000000000008"), "Users"),
            ["roles"] = (Guid.Parse("30000000-0000-0000-0000-000000000009"), "Roles"),
        };

    public PermissionSeeder(IamDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        var moduleCache = new Dictionary<string, PermissionModule>();

        foreach (var permissionName in Permissions.All)
        {
            var moduleCode = ExtractModuleCode(permissionName);

            if (!moduleCache.ContainsKey(moduleCode))
            {
                var module = await _db.PermissionModules
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(m => m.Code == moduleCode);

                if (module is null)
                {
                    WellKnownModules.TryGetValue(
                        moduleCode,
                        out var meta);

                    module = new PermissionModule
                    {
                        Id = meta.Id != Guid.Empty ? meta.Id : Guid.NewGuid(),
                        Name = string.IsNullOrEmpty(meta.Name)
                            ? moduleCode  // fallback: use code as name
                            : meta.Name,
                        Code = moduleCode
                    };

                    _db.PermissionModules.Add(module);
                    await _db.SaveChangesAsync();
                }

                moduleCache[moduleCode] = module;
            }

            var exists = await _db.Permissions
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Name == permissionName);

            if (exists) continue;

            _db.Permissions.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Name = permissionName,
                PermissionModuleId = moduleCache[moduleCode].Id
            });
        }

        await _db.SaveChangesAsync();
    }

    // OCP: derives code from first segment of dotted permission name
    // "catalog.products.view" => "catalog"
    // "new_module.action"     => "new_module"  (no code change needed)
    private static string ExtractModuleCode(string permissionName)
    {
        var dot = permissionName.IndexOf('.');
        return dot < 0 ? permissionName : permissionName[..dot];
    }
}