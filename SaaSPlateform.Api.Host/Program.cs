using Microsoft.EntityFrameworkCore;
using SaaSPlateform.Api.Host.Middleware;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Infrastructure.Persistence.Seed;
using SaaSPlatform.Infrastructure.Services;
using SaaSPlatform.Infrastructure.Services.Billing;


var builder = WebApplication.CreateBuilder(args);



// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // ✅ REQUIRED
builder.Services.AddScoped<ISubscriptionAccessService, SubscriptionAccessService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<
    ITenantContext,
    TenantContext>();

builder.Services.AddScoped<
    ITenantAccessService,
    TenantAccessService>();

var provider = builder.Configuration["DatabaseProvider"];

builder.Services.AddDbContext<SaaSPlatformDbContext>(options =>
{
    if (provider == "Postgres")
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
    }
    else if (provider == "SqlServer")
    {
        //options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
    }
    else
    {
        throw new Exception("Invalid database provider");
    }
});




var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<SaaSPlatformDbContext>();

    await DbSeeder.SeedAsync(db);
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<SubscriptionMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();