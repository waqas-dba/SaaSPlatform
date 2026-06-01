using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.Catalog.Models
{
    public class CreateAddonItem
    {
        public string Name { get; set; } = default!;
        public decimal AdditionalPrice { get; set; }
    }
}
