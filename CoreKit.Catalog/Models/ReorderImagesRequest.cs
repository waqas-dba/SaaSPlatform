using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.Catalog.Models
{
    public class ReorderImagesRequest
    {
        public List<ImageOrderItem> Images { get; set; } = new();
    }
}
