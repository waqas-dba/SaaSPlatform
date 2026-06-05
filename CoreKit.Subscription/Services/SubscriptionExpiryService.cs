// CoreKit.Subscription | Services/SubscriptionExpiryService.cs
using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreKit.Subscription.Services;

public sealed class SubscriptionExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpiryService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);

    public SubscriptionExpiryService(IServiceScopeFactory scopeFactory, ILogger<SubscriptionExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();

                var now = DateTime.UtcNow;
                var expired = await db.TenantSubscriptions
                    .Where(ts => ts.Status == SubscriptionStatus.Active && ts.EndDate != null && ts.EndDate <= now)
                    .ToListAsync(stoppingToken);

                if (expired.Any())
                {
                    foreach (var sub in expired)
                        sub.Status = SubscriptionStatus.Expired;

                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Marked {Count} subscription(s) as expired.", expired.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during subscription expiry check.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}