using CoreKit.IAM.Extensions;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence.Seeders;
using CoreKit.IAM.Services;   // ✅ Use IAM's EncryptionService, not Infrastructure
using CoreKit.Infrastructure.Middleware;
using CoreKit.Tenant.Abstractions;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ✅ Rate limiting for login/refresh endpoints
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", cfg =>
    {
        cfg.PermitLimit = 5;
        cfg.Window = TimeSpan.FromMinutes(1);
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

var connectionString = builder.Configuration.GetConnectionString("Postgres")!;

// ✅ Encryption key – use IAM’s service, delete CoreKit.Infrastructure.Security version
builder.Services.AddSingleton<IEncryptionService>(sp =>
{
    var key = builder.Configuration["EncryptionKey"]!;
    return new EncryptionService(key);  // CoreKit.IAM.Services.EncryptionService
});

builder.Services.AddCoreKitIAM(
    connectionString,
    builder.Configuration.GetSection("Jwt"),
    options =>
    {
        // Only allow default admin seeding in dev
        options.AllowDefaultAdminSeed = builder.Environment.IsDevelopment();
    });

builder.Services.AddTenantKit(connectionString, options =>
{
    options.AutoApproveTenants = false;
    options.AllowMultipleStores = true;   // can be false for single‑store tenants
    options.EnableLegalInfo = true;
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseRateLimiter();                       // ✅ Enable rate limiting
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<IamSeeder>();
        await seeder.SeedAsync();
        Console.WriteLine("✔ IAM Seeder executed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Seeder failed: " + ex.Message);
    }
}

app.Run();