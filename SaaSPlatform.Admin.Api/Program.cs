// SaaSPlatform.Admin.Api | SaaSPlatform.Admin.Api/Program.cs
using CoreKit.IAM.Extensions;
using CoreKit.Infrastructure.Middleware;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
    options.AddFixedWindowLimiter("login", cfg =>
    {
        cfg.PermitLimit = 5;
        cfg.Window = TimeSpan.FromMinutes(1);
    }));

builder.Services.AddHttpContextAccessor();

var connStr = builder.Configuration.GetConnectionString("Postgres")!;

// FIX: IEncryptionService is now registered inside AddCoreKitIAM — no
// duplicate registration needed here.
builder.Services.AddCoreKitIAM(
    connStr,
    builder.Configuration.GetSection("Jwt"),
    options => options.AllowDefaultAdminSeed = builder.Environment.IsDevelopment());

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