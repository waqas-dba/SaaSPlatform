// CoreKit.Catalog/Extensions/ServiceCollectionExtensions.cs
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Persistence;
using CoreKit.Catalog.Services;
using CoreKit.Catalog.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreKit.Catalog.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IProductService, ProductService>();
        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
        return services;
    }
}