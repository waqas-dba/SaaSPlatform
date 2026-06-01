using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.Catalog.Models
{
    public class AttributeValueItem
    {
        public Guid TemplateId { get; set; }
        public string Value { get; set; } = default!;
    }

}
