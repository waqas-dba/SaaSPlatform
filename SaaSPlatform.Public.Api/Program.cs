using CoreKit.IAM.Extensions;
using CoreKit.IAM.Persistence.Seeders;
using CoreKit.Infrastructure.Middleware;
using CoreKit.Tenant.Abstractions;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using Microsoft.AspNetCore.RateLimiting; // ✅ added for tenant resolution

var builder = WebApplication.CreateBuilder(args);

// ✅ Rate limiting for authentication endpoints
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

builder.Services.AddCoreKitIAM(
    connectionString,
    builder.Configuration.GetSection("Jwt"),
    options =>
    {
        options.EnableUserDocuments = true;
        options.EnableUserIdentities = true;
        options.RequireCnic = false;
        options.AllowDefaultAdminSeed = builder.Environment.IsDevelopment(); // ✅ optional
    });

builder.Services.AddTenantKit(connectionString, options =>
{
    options.AutoApproveTenants = false;
    options.AllowMultipleStores = true;
    options.EnableLegalInfo = true;
});

builder.Services.AddControllers();

var app = builder.Build();

// ✅ Run the seeder (base permissions) early
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IamSeeder>();
    await seeder.SeedAsync();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseRateLimiter();                        // ✅ Enable rate limiting
app.UseMiddleware<TenantResolutionMiddleware>(); // ✅ Resolve tenant from header
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();