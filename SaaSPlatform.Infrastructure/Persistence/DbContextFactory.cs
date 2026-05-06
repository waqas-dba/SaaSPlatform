using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class DbContextFactory : IDesignTimeDbContextFactory<SaaSPlatformDbContext>
{
    public SaaSPlatformDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())   // <-- add this
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var provider = config["DatabaseProvider"];

        var optionsBuilder = new DbContextOptionsBuilder<SaaSPlatformDbContext>();

        if (provider == "Postgres")
            optionsBuilder.UseNpgsql(config.GetConnectionString("Postgres"));
        else
            throw new Exception("Unsupported database provider");

        return new SaaSPlatformDbContext(optionsBuilder.Options);
    }
}