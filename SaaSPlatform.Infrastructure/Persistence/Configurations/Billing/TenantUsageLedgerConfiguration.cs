using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Billing.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Billing;

public class TenantUsageLedgerConfiguration : IEntityTypeConfiguration<TenantUsageLedger>
{
    public void Configure(EntityTypeBuilder<TenantUsageLedger> builder)
    {
        builder.HasIndex(x => new
        {
            x.TenantId,
            x.UsageMonthYear
        }).IsUnique();
    }
}