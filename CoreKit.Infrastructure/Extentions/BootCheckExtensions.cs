// CoreKit.Infrastructure | Extensions/BootCheckExtensions.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreKit.Infrastructure.Extensions;

public static class BootCheckExtensions
{
    /// <summary>
    /// Resolves services by their full type name string so CoreKit.Infrastructure
    /// does not need a project reference to CoreKit.IAM or CoreKit.Tenant,
    /// which would create a circular dependency.
    /// </summary>
    public static async Task PerformBootCheckAsync(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<WebApplication>>();
        logger.LogInformation("===== SERVICE RESOLUTION CHECK =====");

        try
        {
            using var scope = app.Services.CreateScope();
            var sp = scope.ServiceProvider;

            // Resolve by full type name — no compile-time reference to IAM or Tenant needed
            ResolveByName(sp, logger, "CoreKit.IAM.Interfaces.IEncryptionService");
            ResolveByName(sp, logger, "CoreKit.IAM.Interfaces.IAuthService");
            ResolveByName(sp, logger, "CoreKit.IAM.Interfaces.ICurrentUserService");
            // FIX: IMutableTenantContext now lives in SharedKernel.Tenancy, not CoreKit.Tenant.Services
            ResolveByName(sp, logger, "CoreKit.SharedKernel.Tenancy.IMutableTenantContext");
            // TenantResolutionMiddleware is not registered as a DI service (it's middleware added via UseMiddleware)
            ResolveByName(sp, logger, "CoreKit.Tenant.Middleware.TenantResolutionMiddleware");

            // Database check — resolved generically through IDbContextFactory pattern
            // or directly by scanning registered DbContext types
            await CheckDatabaseAsync(sp, logger);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Boot check failed: {Message}", ex.Message);
        }
        finally
        {
            logger.LogInformation("===== BOOT CHECK COMPLETE =====");
        }
    }

    private static void ResolveByName(
        IServiceProvider sp,
        ILogger logger,
        string fullTypeName)
    {
        // Find the type across all loaded assemblies
        var type = FindType(fullTypeName);

        if (type == null)
        {
            logger.LogWarning("Type not found in loaded assemblies: {Type}", fullTypeName);
            return;
        }

        try
        {
            var service = sp.GetService(type);
            if (service != null)
            {
                logger.LogInformation("{ShortName,-30} OK", type.Name);
            }
            else
            {
                // TenantResolutionMiddleware is intentionally NOT registered as a service,
                // it's added via app.UseMiddleware<T>(). Don't treat as a problem.
                if (fullTypeName.Contains("TenantResolutionMiddleware"))
                    logger.LogInformation("{ShortName,-30} NOT REGISTERED (expected — added via UseMiddleware)", type.Name);
                else
                    logger.LogWarning("{ShortName,-30} NOT REGISTERED", type.Name);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ShortName,-30} FAILED — {Error}", type.Name, ex.Message);
        }
    }

    private static async Task CheckDatabaseAsync(
        IServiceProvider sp,
        ILogger logger)
    {
        logger.LogInformation("Testing database connection...");

        // Find IamDbContext by name — no direct reference needed
        var dbContextType = FindType("CoreKit.IAM.Persistence.IamDbContext");

        if (dbContextType == null)
        {
            logger.LogWarning("IamDbContext type not found — skipping DB check.");
            return;
        }

        var dbContext = sp.GetService(dbContextType);
        if (dbContext == null)
        {
            logger.LogWarning("IamDbContext not registered — skipping DB check.");
            return;
        }

        // Access Database.CanConnectAsync via reflection to avoid compile-time dependency
        try
        {
            // dbContext is a DbContext — get the Database property
            var databaseProp = dbContextType.BaseType is not null
                ? GetPropertyFromHierarchy(dbContextType, "Database")
                : null;

            if (databaseProp == null)
            {
                logger.LogWarning("Could not reflect Database property on IamDbContext.");
                return;
            }

            var database = databaseProp.GetValue(dbContext);
            if (database == null)
            {
                logger.LogWarning("Database property returned null.");
                return;
            }

            // Find CanConnectAsync(CancellationToken) on DatabaseFacade
            var canConnectMethod = database.GetType()
                .GetMethod("CanConnectAsync",
                    new[] { typeof(CancellationToken) });

            if (canConnectMethod == null)
            {
                logger.LogWarning("CanConnectAsync method not found on DatabaseFacade.");
                return;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                var task = (Task<bool>)canConnectMethod.Invoke(
                    database, new object[] { cts.Token })!;

                var canConnect = await task;

                if (canConnect)
                    logger.LogInformation("DATABASE connection             OK");
                else
                    logger.LogCritical("DATABASE: CanConnect returned false. Check credentials.");
            }
            catch (OperationCanceledException)
            {
                logger.LogCritical(
                    "DATABASE: connection timed out after 5 seconds. " +
                    "Check SSL mode or network connectivity.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database check failed: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// Searches all assemblies currently loaded in the AppDomain for a type
    /// matching the given full name. Covers all project assemblies without
    /// requiring a compile-time reference to any of them.
    /// </summary>
    private static Type? FindType(string fullTypeName)
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic)
            .SelectMany(a =>
            {
                try { return a.GetExportedTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .FirstOrDefault(t => t.FullName == fullTypeName);
    }

    private static System.Reflection.PropertyInfo? GetPropertyFromHierarchy(
        Type type, string propertyName)
    {
        var current = type;
        while (current != null)
        {
            var prop = current.GetProperty(
                propertyName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);
            if (prop != null) return prop;
            current = current.BaseType;
        }
        return null;
    }
}