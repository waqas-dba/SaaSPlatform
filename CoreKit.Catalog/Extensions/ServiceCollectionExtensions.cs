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
        Action<IServiceCollection> configureStoreInfo)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Rule 2: Repository registrations belong inside the module
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped < ICategoryRepository, CategoryRepository > ();
        services.AddScoped<IAddonGroupRepository, AddonGroupRepository>();
        services.AddScoped<IVariantGroupRepository, VariantGroupRepository>();
        services.AddScoped<IVariantAttributeTemplateRepository, VariantAttributeTemplateRepository>();

        services.AddScoped < ICategoryService, CategoryService > ();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductVariantService, ProductVariantService>();
        services.AddScoped<IProductAttributeTemplateService, ProductAttributeTemplateService>();
        services.AddScoped<IProductAttributeGroupService, ProductAttributeGroupService>();
        services.AddScoped<IAddonService, AddonService>();
        services.AddScoped<IVariantAttributeTemplateService, VariantAttributeTemplateService>();
        services.AddScoped<IVariantGroupService, VariantGroupService>();
        services.AddScoped<IVariantValidationService, VariantValidationService>();
        // CoreKit.Catalog/Extensions/ServiceCollectionExtensions.cs — add new registrations
        services.AddScoped<IProductImageService, ProductImageService>();
        configureStoreInfo(services);

        services.AddValidatorsFromAssemblyContaining < CreateProductRequestValidator > ();

        return services;
    }
}