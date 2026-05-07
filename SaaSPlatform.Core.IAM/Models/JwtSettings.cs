using System;
using System.Collections.Generic;
using System.Text;

namespace SaaSPlatform.Core.Tenant.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int AccessTokenMinutes { get; set; } = 120;
        public int RefreshTokenDays { get; set; } = 7;
    }
}
