// SaaSPlatform.Tenant.Api | SaaSPlatform.Tenant.Api/Program.cs
using CoreKit.IAM.Extensions;
using CoreKit.Infrastructure.Middleware;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

var connStr = builder.Configuration.GetConnectionString("Postgres")!;

// FIX: IEncryptionService registered inside AddCoreKitIAM — not needed here.
builder.Services.AddCoreKitIAM(connStr, builder.Configuration.GetSection("Jwt"));

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
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();