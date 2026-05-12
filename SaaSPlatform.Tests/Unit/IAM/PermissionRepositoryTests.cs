using Microsoft.EntityFrameworkCore;
using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Infrastructure.Services.IAM;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SaaSPlatform.UnitTests.IAM;

public class PermissionRepositoryTests
{
    private SaaSPlatformDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<SaaSPlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SaaSPlatformDbContext(options);
    }

    [Fact]
    public async Task HasPermission_WhenUserHasPermission_ReturnsTrue()
    {
        // Arrange
        var db = GetDbContext();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Name = "Test User",                   // required
            PasswordHash = "hashed",              // required
            Phone = "123"                         // optional but provided
        };
        var role = new Role { Id = roleId, TenantId = tenantId, Name = "Admin" };
        var module = new PermissionModule { Id = moduleId, Name = "Orders", Code = "orders" };
        var perm = new Permission { Id = permissionId, Name = "orders.create", PermissionModuleId = moduleId };

        db.Users.Add(user);
        db.Roles.Add(role);
        db.PermissionModules.Add(module);
        db.Permissions.Add(perm);
        db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
        db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId, TenantId = tenantId });
        await db.SaveChangesAsync();

        var repo = new PermissionRepository(db);

        // Act
        var hasPerm = await repo.HasPermissionAsync(userId, tenantId, "orders.create");

        // Assert
        Assert.True(hasPerm);
    }

    [Fact]
    public async Task HasPermission_WhenUserDoesNotHavePermission_ReturnsFalse()
    {
        // Arrange
        var db = GetDbContext();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Name = "Test User",
            PasswordHash = "hashed",
            Phone = "123"
        };
        var role = new Role { Id = roleId, TenantId = tenantId, Name = "Admin" };
        var module = new PermissionModule { Id = moduleId, Name = "Orders", Code = "orders" };
        var perm = new Permission { Id = permissionId, Name = "orders.view", PermissionModuleId = moduleId };

        db.Users.Add(user);
        db.Roles.Add(role);
        db.PermissionModules.Add(module);
        db.Permissions.Add(perm);
        db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
        db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId, TenantId = tenantId });
        await db.SaveChangesAsync();

        var repo = new PermissionRepository(db);

        // Act – request a permission the user does not have
        var hasPerm = await repo.HasPermissionAsync(userId, tenantId, "orders.delete");

        // Assert
        Assert.False(hasPerm);
    }

    [Fact]
    public async Task HasPermissionByModule_WhenHasModuleAccess_ReturnsTrue()
    {
        // Arrange
        var db = GetDbContext();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Name = "Test User",
            PasswordHash = "hashed",
            Phone = "123"
        };
        var role = new Role { Id = roleId, TenantId = tenantId, Name = "Admin" };
        var module = new PermissionModule { Id = moduleId, Name = "Orders", Code = "orders" };
        var perm = new Permission { Id = permissionId, Name = "orders.create", PermissionModuleId = moduleId };

        db.Users.Add(user);
        db.Roles.Add(role);
        db.PermissionModules.Add(module);
        db.Permissions.Add(perm);
        db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
        db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId, TenantId = tenantId });
        await db.SaveChangesAsync();

        var repo = new PermissionRepository(db);

        // Act
        var hasModuleAccess = await repo.HasPermissionByModuleAsync(userId, tenantId, "orders");

        // Assert
        Assert.True(hasModuleAccess);
    }

    [Fact]
    public async Task HasPermissionByModule_WhenNoModuleAccess_ReturnsFalse()
    {
        // Arrange
        var db = GetDbContext();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Name = "Test User",
            PasswordHash = "hashed",
            Phone = "123"
        };
        var role = new Role { Id = roleId, TenantId = tenantId, Name = "Admin" };
        var module = new PermissionModule { Id = moduleId, Name = "Orders", Code = "orders" };
        var perm = new Permission { Id = permissionId, Name = "orders.create", PermissionModuleId = moduleId };

        db.Users.Add(user);
        db.Roles.Add(role);
        db.PermissionModules.Add(module);
        db.Permissions.Add(perm);
        db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
        db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId, TenantId = tenantId });
        await db.SaveChangesAsync();

        var repo = new PermissionRepository(db);

        // Act – request a different module
        var hasModuleAccess = await repo.HasPermissionByModuleAsync(userId, tenantId, "billing");

        // Assert
        Assert.False(hasModuleAccess);
    }
}