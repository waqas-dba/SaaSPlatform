using Microsoft.AspNetCore.Http;
using Moq;
using SaaSPlateform.Api.Host.Middleware;
using SaaSPlatform.Core.Billing.Interfaces;
using System;
using System.Threading.Tasks;

namespace SaaSPlatform.Tests.Unit.Api.Middleware;

public class SubscriptionMiddlewareTests
{
    [Fact]
    public async Task Skips_When_TenantId_Not_Present()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var service = new Mock<ISubscriptionAccessService>();

        var middleware = new SubscriptionMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context, service.Object);

        // Assert
        Assert.NotEqual(StatusCodes.Status402PaymentRequired, context.Response.StatusCode);
    }

    [Fact]
    public async Task Returns_402_When_Subscription_Not_Allowed()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var tenantId = Guid.NewGuid();

        context.Items["TenantId"] = tenantId;

        var service = new Mock<ISubscriptionAccessService>();
        service
            .Setup(x => x.IsTenantAllowedAsync(tenantId))
            .ReturnsAsync(false);

        var middleware = new SubscriptionMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context, service.Object);

        // Assert
        Assert.Equal(StatusCodes.Status402PaymentRequired, context.Response.StatusCode);
    }

    [Fact]
    public async Task Calls_Next_When_Subscription_Allowed()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var tenantId = Guid.NewGuid();

        context.Items["TenantId"] = tenantId;

        var service = new Mock<ISubscriptionAccessService>();
        service
            .Setup(x => x.IsTenantAllowedAsync(tenantId))
            .ReturnsAsync(true);

        var nextCalled = false;

        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new SubscriptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context, service.Object);

        // Assert
        Assert.True(nextCalled);
    }
}