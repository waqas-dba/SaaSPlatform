// CoreKit.Catalog/Extensions/ServiceCollectionExtensions.cs
using CoreKit.Catalog.Abstractions;
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
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        string connectionString,
        Action<IServiceCollection>? configureStoreInfo = null)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductVariantService, ProductVariantService>();

        // Attribute template system.
        services.AddScoped<IProductAttributeTemplateService,
            ProductAttributeTemplateService>();
        services.AddScoped<IProductAttributeGroupService,
            ProductAttributeGroupService>();

        if (configureStoreInfo != null)
            configureStoreInfo(services);
        else
            services.AddScoped<IStoreInfoProvider, TenantDbStoreInfoProvider>();

        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

        return services;
    }
}