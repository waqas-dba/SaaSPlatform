using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.Catalog.Models
{
    public class CreateImageItem
    {
        public string ImageUrl { get; set; } = default!;
        public bool IsPrimary { get; set; }
    }
}
