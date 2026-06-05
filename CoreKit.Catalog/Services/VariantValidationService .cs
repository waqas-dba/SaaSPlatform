// CoreKit.Catalog/Services/VariantValidationService.cs
using System.Text.Json;
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Services;

public class VariantValidationService : IVariantValidationService
{
    private readonly IVariantGroupRepository _variantGroupRepo;
    private readonly IVariantAttributeTemplateRepository _variantTemplateRepo;

    public VariantValidationService(
        IVariantGroupRepository variantGroupRepo,
        IVariantAttributeTemplateRepository variantTemplateRepo)
    {
        _variantGroupRepo = variantGroupRepo;
        _variantTemplateRepo = variantTemplateRepo;
    }

    public async Task ValidateVariantsAgainstGroupAsync(
        Guid variantGroupId,
        Guid tenantId,
        List<CreateVariantRequest> variants,
        CancellationToken ct = default)
    {
        var group = await _variantGroupRepo.GetByIdWithOptionsAsync(
            variantGroupId, tenantId, ct)
            ?? throw new KeyNotFoundException(
                "Variant group not found or does not belong to this tenant.");

        var optionMap = BuildOptionMap(group);

        foreach (var variant in variants)
        {
            foreach (var attr in variant.Attributes)
            {
                // Resolve without mutating attr
                var templateId = await ResolveTemplateIdAsync(
                    attr, group.StoreTypeCode, ct);

                if (!optionMap.ContainsKey(templateId))
                    throw new InvalidOperationException(
                        $"Variant attribute template '{templateId}' is not part " +
                        "of the selected variant group.");

                var allowed = optionMap[templateId];
                if (allowed != null && !allowed.Contains(attr.Value))
                    throw new InvalidOperationException(
                        $"Value '{attr.Value}' is not allowed for this variant option.");
            }
        }
    }

    public async Task<VariantAttributeTemplate> ResolveTemplateAsync(
        VariantAttributeItem attr,
        string storeTypeCode,
        CancellationToken ct = default)
    {
        if (attr.TemplateId.HasValue)
            return await _variantTemplateRepo.GetByIdAsync(attr.TemplateId.Value, ct)
                ?? throw new InvalidOperationException(
                    $"Variant template '{attr.TemplateId}' not found.");

        return await _variantTemplateRepo.GetByNameAndStoreTypeAsync(
            attr.Name, storeTypeCode, ct)
            ?? throw new InvalidOperationException(
                $"Variant attribute '{attr.Name}' not defined " +
                $"for store type '{storeTypeCode}'.");
    }

    private static Dictionary<Guid, List<string>?> BuildOptionMap(VariantGroup group)
        => group.Options.ToDictionary(
            o => o.TemplateId,
            o => string.IsNullOrWhiteSpace(o.AllowedValuesJson)
                ? null
                : JsonSerializer.Deserialize<List<string>>(o.AllowedValuesJson));

    // Returns the resolved template ID without mutating the attr object
    private async Task<Guid> ResolveTemplateIdAsync(
        VariantAttributeItem attr,
        string storeTypeCode,
        CancellationToken ct)
    {
        if (attr.TemplateId.HasValue)
            return attr.TemplateId.Value;

        var template = await _variantTemplateRepo
            .GetByNameAndStoreTypeAsync(attr.Name, storeTypeCode, ct)
            ?? throw new InvalidOperationException(
                $"Variant attribute '{attr.Name}' not found " +
                $"for store type '{storeTypeCode}'.");

        return template.Id;
    }
}