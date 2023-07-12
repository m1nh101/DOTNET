using Redis.OM;
using Redis.OM.Contracts;

namespace ExternalAuthentication.Store;

public class IndexCreationStore : IHostedService
{
    private readonly IRedisConnectionProvider _provider;

    public IndexCreationStore(IRedisConnectionProvider provider)
    {
        _provider = provider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _provider.Connection.CreateIndexAsync(typeof(UserToken));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
