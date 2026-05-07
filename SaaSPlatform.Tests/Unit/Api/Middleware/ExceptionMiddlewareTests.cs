
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SaaSPlateform.Api.Host.Middleware;


namespace SaaSPlatform.Tests.Unit.Api.Middleware;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task Returns_500_When_Exception_Occurs()
    {
        // Arrange
        var context = new DefaultHttpContext();

        RequestDelegate next = _ =>
        {
            throw new Exception("Test exception");
        };

        var logger = new Mock<ILogger<ExceptionMiddleware>>();

        var middleware = new ExceptionMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task Calls_Next_When_No_Exception()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var nextCalled = false;

        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = new Mock<ILogger<ExceptionMiddleware>>();

        var middleware = new ExceptionMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }
}