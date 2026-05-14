using CoreKit.IAM.Extensions;
using CoreKit.IAM.Interfaces;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using SaaSPlatform.Host.Api.Middleware;
using SaaSPlatform.Host.Api.Services;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("Default")!;

// ---------- Encryption (must be before AddCoreKitIAM) ----------
builder.Services.AddSingleton<IEncryptionService>(sp =>
{
    var key = builder.Configuration["EncryptionKey"]
        ?? throw new Exception("EncryptionKey is missing in configuration.");
    return new EncryptionService(key);
});

// ---------- IAM module ----------
builder.Services.AddCoreKitIAM(
    connectionString,
    builder.Configuration.GetSection("Jwt"),
    options =>
    {
        options.EnableUserDocuments = true;
        options.EnableUserIdentities = true;
        options.RequireCnic = true;
    });

// ---------- Tenant module ----------
builder.Services.AddCoreKitTenant(
    connectionString,
    options =>
    {
        options.AutoApproveTenants = false;
        options.AllowMultipleStores = true;
        options.EnableLegalInfo = true;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

var app = builder.Build();

// ---------- Middleware pipeline ----------
app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();