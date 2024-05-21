using Microsoft.Extensions.Configuration;
using Neo4j.Driver;

public class Neo4jService : IDisposable
{
    private readonly IDriver _driver;

    public Neo4jService(IConfiguration configuration)
    {
        var uri = configuration["Neo4j:Uri"];
        var username = configuration["Neo4j:Username"];
        var password = configuration["Neo4j:Password"];

        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
    }

    public IAsyncSession GetSession()
    {
        return _driver.AsyncSession();
    }

    public void Dispose()
    {
        _driver?.Dispose();
    }
}
