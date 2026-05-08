// SaaSPlatform.Infrastructure/Persistence/Configurations/Catalog/CuisineConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;

public class CuisineConfiguration : IEntityTypeConfiguration<Cuisine>
{
    public void Configure(EntityTypeBuilder<Cuisine> builder)
    {
        builder.HasIndex(c => c.Name).IsUnique();
    }
}