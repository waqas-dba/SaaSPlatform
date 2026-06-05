using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.Catalog.Interfaces;

public interface IVariantValidationService
{
    Task ValidateVariantsAgainstGroupAsync(
        Guid variantGroupId,
        Guid tenantId,
        List<CreateVariantRequest> variants,
        CancellationToken ct = default);

    Task<VariantAttributeTemplate> ResolveTemplateAsync(
        VariantAttributeItem attr,
        string storeTypeCode,
        CancellationToken ct = default);
}
