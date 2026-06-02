// CoreKit.IAM/Persistence/Seeders/PermissionSeeder.cs
using CoreKit.IAM.Constants;
using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class PermissionSeeder
{
    private readonly IamDbContext _db;

    public PermissionSeeder(IamDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        // ── Ensure every required module exists (idempotent) ──────────────
        var moduleMap = new Dictionary<string, PermissionModule>();

        foreach (var permissionName in Permissions.All)
        {
            // Determine module code from permission prefix
            var moduleCode = GetModuleCode(permissionName);

            if (!moduleMap.ContainsKey(moduleCode))
            {
                var module = await _db.PermissionModules
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(m => m.Code == moduleCode);

                if (module == null)
                {
                    module = new PermissionModule
                    {
                        Id = moduleCode switch
                        {
                            "iam" => Guid.Parse("30000000-0000-0000-0000-000000000004"),
                            "catalog" => Guid.Parse("30000000-0000-0000-0000-000000000005"),
                            _ => Guid.NewGuid()
                        },
                        Name = moduleCode switch
                        {
                            "iam" => "IAM",
                            "catalog" => "Catalog",
                            _ => moduleCode
                        },
                        Code = moduleCode
                    };
                    _db.PermissionModules.Add(module);
                    await _db.SaveChangesAsync();   // ensure module exists before permissions
                }
                moduleMap[moduleCode] = module;
            }
        }

        // ── Seed permissions under the correct module ────────────────────
        foreach (var permissionName in Permissions.All)
        {
            var exists = await _db.Permissions
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Name == permissionName);

            if (exists) continue;

            var module = moduleMap[GetModuleCode(permissionName)];

            _db.Permissions.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Name = permissionName,
                PermissionModuleId = module.Id
            });
        }

        await _db.SaveChangesAsync();
    }

    // ── Simple prefix → module code mapping ──────────────────────────────
    private static string GetModuleCode(string permissionName)
    {
        // Catalog module permissions
        if (permissionName.StartsWith("catalog.")) return "catalog";

        // All other permissions (including platform.*) belong to IAM for now
        return "iam";
    }
}