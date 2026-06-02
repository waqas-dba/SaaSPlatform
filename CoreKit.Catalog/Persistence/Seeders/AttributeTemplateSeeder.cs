// CoreKit.Catalog/Persistence/Seeders/AttributeTemplateSeeder.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence.Seeders;

/// <summary>
/// Seeds the platform-level base attribute templates for every store type.
/// Run once at startup or as part of the migration pipeline.
/// All rows have TenantId = null (platform scope).
/// </summary>
public sealed class AttributeTemplateSeeder
{
    private readonly CatalogDbContext _db;

    public AttributeTemplateSeeder(CatalogDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Skip if platform templates already exist.
        if (await _db.AttributeTemplates.AnyAsync(t => t.TenantId == null, ct))
            return;

        var groups = BuildGroups();
        _db.AttributeGroups.AddRange(groups.Values.SelectMany(v => v));

        var templates = BuildTemplates(groups);
        _db.AttributeTemplates.AddRange(templates);

        await _db.SaveChangesAsync(ct);
    }

    // ── Groups per store type ─────────────────────────────────────────────

    private static Dictionary<string, List<ProductAttributeGroup>> BuildGroups() =>
        new()
        {
            ["restaurant"] = new()
            {
                G("restaurant", "Basic Info",       1),
                G("restaurant", "Nutritional Info", 2),
                G("restaurant", "Serving",          3)
            },
            ["grocery"] = new()
            {
                G("grocery", "Product Info",   1),
                G("grocery", "Nutrition",      2),
                G("grocery", "Storage",        3)
            },
            ["clothing"] = new()
            {
                G("clothing", "Sizing",        1),
                G("clothing", "Material",      2),
                G("clothing", "Care",          3)
            },
            ["pharmacy"] = new()
            {
                G("pharmacy", "Drug Info",     1),
                G("pharmacy", "Dosage",        2),
                G("pharmacy", "Storage",       3)
            },
            ["electronics"] = new()
            {
                G("electronics", "Specs",      1),
                G("electronics", "Connectivity",2),
                G("electronics", "Warranty",   3)
            },
            ["hotel"] = new()
            {
                G("hotel", "Room Info",        1),
                G("hotel", "Amenities",        2),
                G("hotel", "Policies",         3)
            },
            ["salon"] = new()
            {
                G("salon", "Service Info",     1),
                G("salon", "Requirements",     2)
            },
            ["bakery"] = new()
            {
                G("bakery", "Product Info",    1),
                G("bakery", "Allergens",       2)
            },
            ["cafe"] = new()
            {
                G("cafe", "Drink Info",        1),
                G("cafe", "Customisation",     2)
            },
            ["bookstore"] = new()
            {
                G("bookstore", "Book Details", 1),
                G("bookstore", "Publishing",   2)
            }
        };

    // ── Templates per store type ──────────────────────────────────────────

    private static List<ProductAttributeTemplate> BuildTemplates(
        Dictionary<string, List<ProductAttributeGroup>> groups)
    {
        var all = new List<ProductAttributeTemplate>();

        all.AddRange(Restaurant(groups["restaurant"]));
        all.AddRange(Grocery(groups["grocery"]));
        all.AddRange(Clothing(groups["clothing"]));
        all.AddRange(Pharmacy(groups["pharmacy"]));
        all.AddRange(Electronics(groups["electronics"]));
        all.AddRange(Hotel(groups["hotel"]));
        all.AddRange(Salon(groups["salon"]));
        all.AddRange(Bakery(groups["bakery"]));
        all.AddRange(Cafe(groups["cafe"]));
        all.AddRange(Bookstore(groups["bookstore"]));

        return all;
    }

    private static List<ProductAttributeTemplate> Restaurant(
        List<ProductAttributeGroup> g) =>
    new()
    {
        T("restaurant", "Spice Level",
            AttributeFieldType.Select,
            "[\"Mild\",\"Medium\",\"Hot\",\"Extra Hot\"]",
            isRequired: false, g[0], 1),
        T("restaurant", "Is Vegetarian",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[0], 2),
        T("restaurant", "Is Vegan",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[0], 3),
        T("restaurant", "Is Halal",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[0], 4),
        T("restaurant", "Cuisine Type",
            AttributeFieldType.Select,
            "[\"Italian\",\"Chinese\",\"Pakistani\",\"Fast Food\",\"Continental\",\"Other\"]",
            isRequired: true, g[0], 5),
        T("restaurant", "Calories",
            AttributeFieldType.Number, null,
            isRequired: false, g[1], 1),
        T("restaurant", "Allergens",
            AttributeFieldType.Text, null,
            isRequired: false, g[1], 2),
        T("restaurant", "Serving Size",
            AttributeFieldType.Text, null,
            isRequired: false, g[2], 1),
        T("restaurant", "Preparation Time (mins)",
            AttributeFieldType.Number, null,
            isRequired: false, g[2], 2)
    };

    private static List<ProductAttributeTemplate> Grocery(
        List<ProductAttributeGroup> g) =>
    new()
    {
        T("grocery", "Brand",
            AttributeFieldType.Text, null,
            isRequired: false, g[0], 1),
        T("grocery", "Weight (g)",
            AttributeFieldType.Number, null,
            isRequired: true, g[0], 2),
        T("grocery", "Barcode",
            AttributeFieldType.Text, null,
            isRequired: false, g[0], 3),
        T("grocery", "Is Organic",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[0], 4),
        T("grocery", "Calories per 100g",
            AttributeFieldType.Number, null,
            isRequired: false, g[1], 1),
        T("grocery", "Ingredients",
            AttributeFieldType.Text, null,
            isRequired: false, g[1], 2),
        T("grocery", "Allergens",
            AttributeFieldType.Text, null,
            isRequired: false, g[1], 3),
        T("grocery", "Storage Temperature",
            AttributeFieldType.Select,
            "[\"Ambient\",\"Chilled\",\"Frozen\"]",
            isRequired: true, g[2], 1),
        T("grocery", "Expiry Type",
            AttributeFieldType.Select,
            "[\"Best Before\",\"Use By\"]",
            isRequired: false, g[2], 2)
    };

    private static List<ProductAttributeTemplate> Clothing(
        List<ProductAttributeGroup> g) =>
    new()
    {
        T("clothing", "Size",
            AttributeFieldType.Select,
            "[\"XS\",\"S\",\"M\",\"L\",\"XL\",\"XXL\"]",
            isRequired: true, g[0], 1),
        T("clothing", "Size Guide URL",
            AttributeFieldType.Text, null,
            isRequired: false, g[0], 2),
        T("clothing", "Fabric",
            AttributeFieldType.Text, null,
            isRequired: true, g[1], 1),
        T("clothing", "Color",
            AttributeFieldType.Text, null,
            isRequired: true, g[1], 2),
        T("clothing", "Gender",
            AttributeFieldType.Select,
            "[\"Men\",\"Women\",\"Unisex\",\"Kids\"]",
            isRequired: true, g[1], 3),
        T("clothing", "Wash Instructions",
            AttributeFieldType.Text, null,
            isRequired: false, g[2], 1),
        T("clothing", "Dry Clean Only",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[2], 2)
    };

    private static List<ProductAttributeTemplate> Pharmacy(
        List<ProductAttributeGroup> g) =>
    new()
    {
        T("pharmacy", "Generic Name",
            AttributeFieldType.Text, null,
            isRequired: true, g[0], 1),
        T("pharmacy", "Manufacturer",
            AttributeFieldType.Text, null,
            isRequired: true, g[0], 2),
        T("pharmacy", "Requires Prescription",
            AttributeFieldType.Boolean, null,
            isRequired: true, g[0], 3),
        T("pharmacy", "Drug Class",
            AttributeFieldType.Text, null,
            isRequired: false, g[0], 4),
        T("pharmacy", "Dosage Form",
            AttributeFieldType.Select,
            "[\"Tablet\",\"Capsule\",\"Syrup\",\"Injection\",\"Cream\",\"Drops\"]",
            isRequired: true, g[1], 1),
        T("pharmacy", "Strength (mg)",
            AttributeFieldType.Number, null,
            isRequired: true, g[1], 2),
        T("pharmacy", "Dosage Instructions",
            AttributeFieldType.Text, null,
            isRequired: false, g[1], 3),
        T("pharmacy", "Storage Condition",
            AttributeFieldType.Select,
            "[\"Below 25°C\",\"Refrigerate 2-8°C\",\"Protect from Light\"]",
            isRequired: true, g[2], 1)
    };

    private static List<ProductAttributeTemplate> Electronics(
        List<ProductAttributeGroup> g) =>
    new()
    {
        T("electronics", "Brand",
            AttributeFieldType.Text, null,
            isRequired: true, g[0], 1),
        T("electronics", "Model Number",
            AttributeFieldType.Text, null,
            isRequired: true, g[0], 2),
        T("electronics", "Color",
            AttributeFieldType.Text, null,
            isRequired: false, g[0], 3),
        T("electronics", "Power (W)",
            AttributeFieldType.Number, null,
            isRequired: false, g[0], 4),
        T("electronics", "Bluetooth",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[1], 1),
        T("electronics", "WiFi",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[1], 2),
        T("electronics", "Ports",
            AttributeFieldType.Text, null,
            isRequired: false, g[1], 3),
        T("electronics", "Warranty (months)",
            AttributeFieldType.Number, null,
            isRequired: true, g[2], 1),
        T("electronics", "Warranty Type",
            AttributeFieldType.Select,
            "[\"Local\",\"International\",\"Brand\",\"Seller\"]",
            isRequired: false, g[2], 2)
    };

    private static List<ProductAttributeTemplate> Hotel(
        List<ProductAttributeGroup> g) =>
    new()
    {
        T("hotel", "Room Type",
            AttributeFieldType.Select,
            "[\"Single\",\"Double\",\"Suite\",\"Deluxe\",\"Family\"]",
            isRequired: true, g[0], 1),
        T("hotel", "Max Occupancy",
            AttributeFieldType.Number, null,
            isRequired: true, g[0], 2),
        T("hotel", "Floor",
            AttributeFieldType.Number, null,
            isRequired: false, g[0], 3),
        T("hotel", "Bed Type",
            AttributeFieldType.Select,
            "[\"Single\",\"Double\",\"King\",\"Twin\"]",
            isRequired: true, g[0], 4),
        T("hotel", "Air Conditioning",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[1], 1),
        T("hotel", "Free WiFi",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[1], 2),
        T("hotel", "Breakfast Included",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[1], 3),
        T("hotel", "Cancellation Policy",
            AttributeFieldType.Select,
            "[\"Free Cancellation\",\"Non-Refundable\",\"Partial Refund\"]",
            isRequired: true, g[2], 1),
        T("hotel", "Check-in Time",
            AttributeFieldType.Text, null,
            isRequired: false, g[2], 2)
    };

    private static List<ProductAttributeTemplate> Salon(
        List<ProductAttributeGroup> g) =>
    new()
    {
        T("salon", "Service Category",
            AttributeFieldType.Select,
            "[\"Hair\",\"Skin\",\"Nails\",\"Makeup\",\"Spa\"]",
            isRequired: true, g[0], 1),
        T("salon", "Duration (mins)",
            AttributeFieldType.Number, null,
            isRequired: true, g[0], 2),
        T("salon", "Gender",
            AttributeFieldType.Select,
            "[\"Men\",\"Women\",\"Unisex\"]",
            isRequired: true, g[0], 3),
        T("salon", "Products Used",
            AttributeFieldType.Text, null,
            isRequired: false, g[0], 4),
        T("salon", "Patch Test Required",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[1], 1),
        T("salon", "Minimum Age",
            AttributeFieldType.Number, null,
            isRequired: false, g[1], 2)
    };

    private static List<ProductAttributeTemplate> Bakery(
        List<ProductAttributeGroup> g) =>
    new()
    {
        T("bakery", "Is Gluten Free",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[0], 1),
        T("bakery", "Is Sugar Free",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[0], 2),
        T("bakery", "Weight (g)",
            AttributeFieldType.Number, null,
            isRequired: false, g[0], 3),
        T("bakery", "Shelf Life (days)",
            AttributeFieldType.Number, null,
            isRequired: false, g[0], 4),
        T("bakery", "Contains Nuts",
            AttributeFieldType.Boolean, null,
            isRequired: true, g[1], 1),
        T("bakery", "Contains Dairy",
            AttributeFieldType.Boolean, null,
            isRequired: true, g[1], 2),
        T("bakery", "Contains Eggs",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[1], 3)
    };

    private static List<ProductAttributeTemplate> Cafe(
        List<ProductAttributeGroup> g) =>
    new()
    {
        T("cafe", "Drink Type",
            AttributeFieldType.Select,
            "[\"Coffee\",\"Tea\",\"Juice\",\"Smoothie\",\"Shake\",\"Other\"]",
            isRequired: true, g[0], 1),
        T("cafe", "Is Hot",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[0], 2),
        T("cafe", "Caffeine Free",
            AttributeFieldType.Boolean, null,
            isRequired: false, g[0], 3),
        T("cafe", "Milk Options",
            AttributeFieldType.Select,
            "[\"Full Fat\",\"Skimmed\",\"Oat\",\"Soy\",\"Almond\",\"None\"]",
            isRequired: false, g[1], 1),
        T("cafe", "Sugar Options",
            AttributeFieldType.Select,
            "[\"Regular\",\"Low\",\"None\",\"Sweetener\"]",
            isRequired: false, g[1], 2),
        T("cafe", "Size Options",
            AttributeFieldType.Select,
            "[\"Small\",\"Medium\",\"Large\"]",
            isRequired: false, g[1], 3)
    };

    private static List<ProductAttributeTemplate> Bookstore(
        List<ProductAttributeGroup> g) =>
    new()
    {
        T("bookstore", "Author",
            AttributeFieldType.Text, null,
            isRequired: true, g[0], 1),
        T("bookstore", "Genre",
            AttributeFieldType.Select,
            "[\"Fiction\",\"Non-Fiction\",\"Science\",\"History\",\"Biography\",\"Children\",\"Other\"]",
            isRequired: true, g[0], 2),
        T("bookstore", "Language",
            AttributeFieldType.Text, null,
            isRequired: true, g[0], 3),
        T("bookstore", "Pages",
            AttributeFieldType.Number, null,
            isRequired: false, g[0], 4),
        T("bookstore", "Format",
            AttributeFieldType.Select,
            "[\"Hardcover\",\"Paperback\",\"eBook\",\"Audiobook\"]",
            isRequired: true, g[0], 5),
        T("bookstore", "ISBN",
            AttributeFieldType.Text, null,
            isRequired: false, g[1], 1),
        T("bookstore", "Publisher",
            AttributeFieldType.Text, null,
            isRequired: false, g[1], 2),
        T("bookstore", "Edition",
            AttributeFieldType.Text, null,
            isRequired: false, g[1], 3)
    };

    // ── Factory helpers ───────────────────────────────────────────────────

    private static ProductAttributeGroup G(
        string storeTypeCode, string name, int sortOrder) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            StoreTypeCode = storeTypeCode,
            SortOrder = sortOrder,
            TenantId = null
        };

    private static ProductAttributeTemplate T(
        string storeTypeCode,
        string name,
        AttributeFieldType fieldType,
        string? optionsJson,
        bool isRequired,
        ProductAttributeGroup group,
        int sortOrder) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            StoreTypeCode = storeTypeCode,
            FieldType = fieldType,
            OptionsJson = optionsJson,
            IsRequired = isRequired,
            IsVisible = true,
            SortOrder = sortOrder,
            TenantId = null,
            GroupId = group.Id
        };
}