using Redis.OM;
using Redis.OM.Contracts;

namespace ExternalAuthentication.Store;

public static class UserTokenStoreConfiguration
{
    public static IServiceCollection ConfigureUserTokenStore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<IndexCreationStore>();

        services.AddSingleton<IRedisConnectionProvider>(sp =>
        {
            var connection = new RedisConnectionConfiguration()
            {
                Host = configuration["Redis:Host"] ?? throw new NullReferenceException(),
                Port = Convert.ToInt32(configuration["Redis:Port"] ?? throw new NullReferenceException()) 
            };

            var provider = new RedisConnectionProvider(connection);

            return provider;
        });

        services.AddSingleton<TokenStore>();

        return services;
    }
}
