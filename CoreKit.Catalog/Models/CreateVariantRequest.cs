using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CoreKit.Catalog.Models;

public class CreateVariantRequest
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int SortOrder { get; set; }
}