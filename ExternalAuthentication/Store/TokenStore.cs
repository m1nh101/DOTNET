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

    public async Task<UserToken> GetToken(string id)
    {
        var userToken = await _tokens.FindByIdAsync(id);

        return userToken is null
            ? throw new NullReferenceException()
            : userToken;
    }

    public async Task SaveToken(string id, string token, string? refreshToken)
    {
        var userToken = await _tokens.FindByIdAsync(id);

        if (userToken == null)
        {
            userToken = new UserToken
            {
                Token = token,
                UserId = id.Replace("-", ""),
                RefreshToken = refreshToken,
                LastUpdate = DateTime.UtcNow
            };

            await _tokens.InsertAsync(userToken);
        }

        userToken.Token = token;
        userToken.LastUpdate = DateTime.UtcNow;

        await _tokens.SaveAsync();
    }

    public Task RevokeToken(string id)
    {
        _provider.Connection.Unlink($"Token:{id}");

        return Task.CompletedTask;
    }
}