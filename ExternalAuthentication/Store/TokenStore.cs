using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Searching;

namespace ExternalAuthentication.Store;

public sealed class TokenStore
{
    private readonly IRedisCollection<UserToken> _tokens;
    private readonly IRedisConnectionProvider _provider;

    public TokenStore(IRedisConnectionProvider provider)
    {
        _provider = provider;
        _tokens = provider.RedisCollection<UserToken>();
    }

    public async Task<string> GetToken(string id)
    {
        var userToken = await _tokens.FindByIdAsync(id);

        if (userToken is null) throw new NullReferenceException();

        return userToken.Token;
    }

    public async Task SaveToken(string id, string token)
    {
        var userToken = await _tokens.FindByIdAsync(id);

        if (userToken == null)
        {
            userToken = new UserToken
            {
                Token = token,
                UserId = id.Replace("-", "")
            };

            await _tokens.InsertAsync(userToken);
        }

        userToken.Token = token;

        await _tokens.SaveAsync();
    }

    public Task RevokeToken(string id)
    {
        _provider.Connection.Unlink($"Token:{id}");

        return Task.CompletedTask;
    }
}