using System;
using System.Collections.Generic;
using System.Text;

namespace CoreKit.IAM.Models;

public class CreateUserRequest
{
    public string Name { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public string Password { get; set; } = default!;
    public Guid? TenantId { get; set; }
}