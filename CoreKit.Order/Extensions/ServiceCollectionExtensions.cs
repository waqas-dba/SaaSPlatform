// CoreKit.Order/Extensions/ServiceCollectionExtensions.cs
using CoreKit.Order.Interfaces;
using CoreKit.Order.Persistence;
using CoreKit.Order.Repository;
using CoreKit.Order.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace CoreKit.Order.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}