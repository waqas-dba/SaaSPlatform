using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.Catalog.Models
{
    public class ImageOrderItem
    {
        public Guid ImageId { get; set; }
        public int SortOrder { get; set; }
    }
}
