using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Billing.Services;

namespace SaaSPlatform.UnitTests.Billing;

public class SubscriptionLifecycleTests
{
    private readonly SubscriptionService _subscriptionService;
    private readonly SubscriptionRuleEngine _ruleEngine;

    public SubscriptionLifecycleTests()
    {
        _subscriptionService = new SubscriptionService();
        _ruleEngine = new SubscriptionRuleEngine();
    }

    private Plan CreatePlan()
    {
        return new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Pro Plan",

            GraceDays = 7,
            TrialDays = 14,

            AllowOrders = true,
            AllowProductCreation = true,
            AllowAdminAccess = true,

            IsActive = true
        };
    }

    [Fact]
    public async Task Trialing_Subscription_Should_Allow_Access()
    {
        // Arrange
        var plan = CreatePlan();

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),

            Status = SubscriptionStatus.Trialing,

            StartDate = DateTime.UtcNow,
            TrialEndsAt = DateTime.UtcNow.AddDays(plan.TrialDays),

            NextBillingDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = await _ruleEngine
            .CanAccessAsync(subscription, plan);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Expired_Trial_Should_Deny_Access()
    {
        // Arrange
        var plan = CreatePlan();

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),

            Status = SubscriptionStatus.Trialing,

            StartDate = DateTime.UtcNow.AddDays(-20),
            TrialEndsAt = DateTime.UtcNow.AddDays(-1),

            NextBillingDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = await _ruleEngine
            .CanAccessAsync(subscription, plan);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Active_Subscription_Should_Allow_Access()
    {
        // Arrange
        var plan = CreatePlan();

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),

            Status = SubscriptionStatus.Active,

            StartDate = DateTime.UtcNow.AddDays(-10),

            NextBillingDate = DateTime.UtcNow.AddDays(20)
        };

        // Act
        var result = await _ruleEngine
            .CanAccessAsync(subscription, plan);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Payment_Failure_Should_Move_To_Grace_Period()
    {
        // Arrange
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),

            Status = SubscriptionStatus.Active,

            StartDate = DateTime.UtcNow.AddMonths(-1),

            NextBillingDate = DateTime.UtcNow
        };

        // Act
        _subscriptionService
            .HandlePaymentFailed(subscription);

        // Assert
        Assert.Equal(
            SubscriptionStatus.GracePeriod,
            subscription.Status);

        Assert.NotNull(subscription.PaymentFailedAt);
    }

    [Fact]
    public async Task Grace_Period_Should_Allow_Access()
    {
        // Arrange
        var plan = CreatePlan();

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),

            Status = SubscriptionStatus.GracePeriod,

            PaymentFailedAt = DateTime.UtcNow.AddDays(-2),

            NextBillingDate = DateTime.UtcNow
        };

        // Act
        var result = await _ruleEngine
            .CanAccessAsync(subscription, plan);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Expired_Grace_Period_Should_Block_Access()
    {
        // Arrange
        var plan = CreatePlan();

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),

            Status = SubscriptionStatus.GracePeriod,

            PaymentFailedAt = DateTime.UtcNow.AddDays(-10),

            NextBillingDate = DateTime.UtcNow.AddDays(-10)
        };

        // Act
        _subscriptionService
            .EvaluateSubscription(subscription, plan);

        var result = await _ruleEngine
            .CanAccessAsync(subscription, plan);

        // Assert
        Assert.False(result);

        Assert.Equal(
            SubscriptionStatus.Suspended,
            subscription.Status);
    }

    [Fact]
    public async Task Suspended_Subscription_Should_Block_Access()
    {
        // Arrange
        var plan = CreatePlan();

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),

            Status = SubscriptionStatus.Suspended,

            StartDate = DateTime.UtcNow.AddMonths(-2),

            NextBillingDate = DateTime.UtcNow.AddMonths(-1)
        };

        // Act
        var result = await _ruleEngine
            .CanAccessAsync(subscription, plan);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Canceled_Subscription_Should_Block_Access()
    {
        // Arrange
        var plan = CreatePlan();

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),

            Status = SubscriptionStatus.Canceled,

            CancelledAt = DateTime.UtcNow,

            NextBillingDate = DateTime.UtcNow
        };

        // Act
        var result = await _ruleEngine
            .CanAccessAsync(subscription, plan);

        // Assert
        Assert.False(result);
    }
}