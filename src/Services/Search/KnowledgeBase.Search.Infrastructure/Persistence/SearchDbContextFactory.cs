using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KnowledgeBase.Search.Infrastructure.Persistence;

public sealed class SearchDbContextFactory : IDesignTimeDbContextFactory<SearchDbContext>
{
    public SearchDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SearchDbContext>()
            .UseNpgsql("Host=localhost;Database=searchdb;Username=postgres;Password=postgres",
                npgsql => npgsql.UseVector())
            .Options;

        return new SearchDbContext(options);
    }
}
