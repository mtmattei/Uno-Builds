using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DiscordGitHubBridge.Data;

/// <summary>Used by the dotnet-ef tooling only. The runtime context comes from DI.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BridgeDbContext>
{
    public BridgeDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BridgeDbContext>()
            .UseSqlite("Data Source=bridge.db")
            .Options;
        return new BridgeDbContext(options);
    }
}
