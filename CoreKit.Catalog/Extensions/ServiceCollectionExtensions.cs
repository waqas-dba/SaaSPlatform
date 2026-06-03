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
    // CoreKit.Catalog/Extensions/ServiceCollectionExtensions.cs
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        string connectionString,
        Action<IServiceCollection> configureStoreInfo)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductVariantService, ProductVariantService>();
        services.AddScoped<IProductAttributeTemplateService,
            ProductAttributeTemplateService>();
        services.AddScoped<IProductAttributeGroupService,
            ProductAttributeGroupService>();
        services.AddScoped<IAddonService, AddonService>();
        services.AddScoped<IVariantAttributeTemplateService,
            VariantAttributeTemplateService>();
        services.AddScoped<IVariantGroupService, VariantGroupService>();

        configureStoreInfo(services);

        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

        return services;
    }
}