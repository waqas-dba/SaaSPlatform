// SaaSPlatform.Public.Api/Program.cs
using CoreKit.IAM.Extensions;
using CoreKit.Infrastructure.Middleware;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// FIX: The rate limiter policy is defined here and applied via
// [EnableRateLimiting("login")] on AuthController. A global limiter
// is not applied because most endpoints shouldn't be throttled.
builder.Services.AddRateLimiter(options =>
    options.AddFixedWindowLimiter("login", cfg =>
    {
        cfg.PermitLimit = 5;
        cfg.Window = TimeSpan.FromMinutes(1);
    }));

builder.Services.AddHttpContextAccessor();

var connStr = builder.Configuration.GetConnectionString("Postgres")!;

builder.Services.AddCoreKitIAM(connStr, builder.Configuration.GetSection("Jwt"),
    options =>
    {
        options.EnableUserDocuments = true;
        options.EnableUserIdentities = true;
        options.RequireCnic = false;
        options.AllowDefaultAdminSeed =
            builder.Environment.IsDevelopment();
    });

// FIX: AddTenantKit now registers TenantContext / ITenantContext /
// IMutableTenantContext internally — don't register ITenantContext again
// here, it would create a second unrelated scoped instance.
builder.Services.AddTenantKit(connStr, options =>
{
    options.AutoApproveTenants = false;
    options.AllowMultipleStores = true;
    options.EnableLegalInfo = true;
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseRateLimiter();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();