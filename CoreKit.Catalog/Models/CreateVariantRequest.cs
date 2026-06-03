using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CoreKit.Catalog.Models;

public class CreateVariantRequest
{
    public string Sku { get; set; } = default!;
    public decimal Price { get; set; }
    public List<VariantAttributeItem> Attributes { get; set; } = new();
}