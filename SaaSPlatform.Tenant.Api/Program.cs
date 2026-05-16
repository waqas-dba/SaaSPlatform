using CoreKit.IAM.Extensions;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence.Seeders;
using CoreKit.Infrastructure.Middleware;
using CoreKit.IAM.Services;
using CoreKit.Tenant.Abstractions;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using CoreKit.Tenant.Persistence.Seeders;
try
{
    var builder = WebApplication.CreateBuilder(args);

    // =========================
    // CORE SERVICES
    // =========================
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ITenantContext, TenantContext>();
    builder.Services.AddScoped<IamSeeder>();
    builder.Services.AddScoped<TenantSeeder>();

    var connectionString =
        builder.Configuration.GetConnectionString("Postgres")!;

    // =========================
    // ENCRYPTION
    // =========================
    builder.Services.AddSingleton<IEncryptionService>(sp =>
    {
        var key = builder.Configuration["EncryptionKey"]!;
        return new EncryptionService(key);
    });

    // =========================
    // IAM MODULE
    // =========================
    builder.Services.AddCoreKitIAM(
        connectionString,
        builder.Configuration.GetSection("Jwt"));

    // =========================
    // TENANT MODULE
    // =========================
    builder.Services.AddTenantKit(connectionString, options =>
    {
        options.AutoApproveTenants = false;
        options.AllowMultipleStores = true;
        options.EnableLegalInfo = true;
    });

    // =========================
    // CONTROLLERS
    // =========================
    builder.Services.AddControllers();

    var app = builder.Build();

    // =========================
    // MIDDLEWARE PIPELINE
    // =========================
    app.UseMiddleware<ExceptionMiddleware>();

    app.UseRouting();

    app.UseMiddleware<TenantResolutionMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // =========================
    // 🔥 CRITICAL FIX: RUN SEEDER
    // =========================
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<IamSeeder>();
        await seeder.SeedAsync();
    }

    app.Run();

}
catch (Exception ex)
{
    Console.WriteLine("=============================================");
    Console.WriteLine(" STARTUP FAILED");
    Console.WriteLine("=============================================");
    Console.WriteLine(ex.ToString());
    Console.WriteLine("=============================================");
    // Do NOT rethrow – we want to see the output before the process exits
    Environment.Exit(1);
}