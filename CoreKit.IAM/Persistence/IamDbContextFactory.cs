// CoreKit.IAM/Persistence/IamDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoreKit.IAM.Persistence;

// THIS CLASS IS FOR DESIGN-TIME TOOLING ONLY (dotnet ef migrations add …).
// It is never instantiated at runtime. The connection string below is a
// LOCAL DEVELOPMENT DEFAULT — never replace it with a production credential.
// Add this file to .gitignore or keep it credential-free; it will be
// compiled into the assembly regardless of build configuration.
public class IamDbContextFactory : IDesignTimeDbContextFactory<IamDbContext>
{
    public IamDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IamDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=127.0.0.1;Port=5432;Database=saas_db;Username=postgres;Password=123;");
        return new IamDbContext(optionsBuilder.Options);
    }
}