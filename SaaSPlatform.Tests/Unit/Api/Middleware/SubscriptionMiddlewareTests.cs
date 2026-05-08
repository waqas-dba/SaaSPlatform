using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SaaSPlatform.Api.Host.Middleware;
using SaaSPlatform.Core.Billing.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SaaSPlatform.UnitTests.Middleware
{
    public class SubscriptionMiddlewareTests
    {
        [Fact]
        public async Task Should_Block_Request_When_Subscription_Not_Allowed()
        {
            // Arrange
            var subscriptionService = new Mock<ISubscriptionAccessService>();

            subscriptionService
                .Setup(x => x.IsTenantAllowedAsync(It.IsAny<Guid>()))
                .ReturnsAsync(false);

            var context = new DefaultHttpContext();
            context.Items["TenantId"] = Guid.NewGuid();

            var middleware = new SubscriptionMiddleware(
                next: (ctx) => Task.CompletedTask,
                logger: Mock.Of<ILogger<SubscriptionMiddleware>>()
            );

            // Act
            await middleware.InvokeAsync(
                context,
                subscriptionService.Object);

            // Assert
            Assert.Equal(StatusCodes.Status402PaymentRequired, context.Response.StatusCode);
        }
    }
}