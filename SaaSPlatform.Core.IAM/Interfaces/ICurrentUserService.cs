using System;
using System.Collections.Generic;
using System.Text;

namespace SaaSPlatform.Core.IAM.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    Guid? TenantId { get; }

    bool IsAuthenticated { get; }

    bool IsSuperAdmin { get; }
}
