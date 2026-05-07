using Microsoft.AspNetCore.Http;
using Moq;
using SaaSPlateform.Api.Host.Middleware;
using SaaSPlatform.Core.Tenant.Interfaces;


namespace SaaSPlatform.Tests.Unit.Api.Middleware;

public class TenantMiddlewareTests
{
    [Fact]
    public async Task Returns_400_When_Header_Is_Missing()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var tenantService = new Mock<ITenantAccessService>();
        var tenantContext = new Mock<ITenantContext>();

        var middleware = new TenantMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.Invoke(context, tenantService.Object, tenantContext.Object);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task Returns_400_When_Invalid_Guid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-ID"] = "invalid-guid";

        var tenantService = new Mock<ITenantAccessService>();
        var tenantContext = new Mock<ITenantContext>();

        var middleware = new TenantMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.Invoke(context, tenantService.Object, tenantContext.Object);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task Returns_404_When_Tenant_Not_Found()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var tenantId = Guid.NewGuid();

        context.Request.Headers["X-Tenant-ID"] = tenantId.ToString();

        var tenantService = new Mock<ITenantAccessService>();
        tenantService
            .Setup(x => x.TenantExistsAsync(tenantId))
            .ReturnsAsync(false);

        var tenantContext = new Mock<ITenantContext>();

        var middleware = new TenantMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.Invoke(context, tenantService.Object, tenantContext.Object);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task Sets_Tenant_And_Calls_Next_When_Valid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var tenantId = Guid.NewGuid();

        context.Request.Headers["X-Tenant-ID"] = tenantId.ToString();

        var tenantService = new Mock<ITenantAccessService>();
        tenantService
            .Setup(x => x.TenantExistsAsync(tenantId))
            .ReturnsAsync(true);

        var tenantContext = new Mock<ITenantContext>();

        var nextCalled = false;

        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantMiddleware(next);

        // Act
        await middleware.Invoke(context, tenantService.Object, tenantContext.Object);

        // Assert
        Assert.True(nextCalled);
        tenantContext.VerifySet(x => x.TenantId = tenantId);
    }
}