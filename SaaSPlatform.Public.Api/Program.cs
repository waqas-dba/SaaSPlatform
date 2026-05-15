using CoreKit.IAM.Extensions;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence.Seeders;
using CoreKit.Infrastructure.Middleware;
using CoreKit.Tenant.Abstractions;
using CoreKit.Tenant.Extensions;

var builder = WebApplication.CreateBuilder(args);

// CORE SERVICES
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

var connectionString = builder.Configuration.GetConnectionString("Postgres")!;

// IAM MODULE
builder.Services.AddCoreKitIAM(
    connectionString,
    builder.Configuration.GetSection("Jwt"),
    options =>
    {
        options.EnableUserDocuments = true;
        options.EnableUserIdentities = true;
        options.RequireCnic = false;
    });

// TENANT MODULE
builder.Services.AddTenantKit(connectionString, options =>
{
    options.AutoApproveTenants = false;
    options.AllowMultipleStores = true;
    options.EnableLegalInfo = true;
});

builder.Services.AddControllers();

var app = builder.Build();

// SEEDER (SAFE)
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IamSeeder>();
    await seeder.SeedAsync();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();