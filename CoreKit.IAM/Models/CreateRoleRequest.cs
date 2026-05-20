using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.IAM.Models;

public class CreateRoleRequest
{
    public string Name { get; set; } = default!;
    public Guid? TenantId { get; set; }
    public string? Description { get; set; }
}
