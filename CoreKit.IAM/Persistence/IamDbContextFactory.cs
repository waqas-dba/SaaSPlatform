using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoreKit.IAM.Persistence;

public class IamDbContextFactory : IDesignTimeDbContextFactory<IamDbContext>
{
    public IamDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IamDbContext>();

        // 🔥 CHANGE THIS CONNECTION STRING FOR MIGRATION ONLY
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=saas_db;Username=postgres;Password=123");

        return new IamDbContext(optionsBuilder.Options);
    }
}