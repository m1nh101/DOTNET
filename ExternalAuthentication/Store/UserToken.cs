using Redis.OM.Modeling;

namespace ExternalAuthentication.Store;

[Document(StorageType = StorageType.Json)]
public sealed class UserToken
{
    [Indexed, RedisIdField]
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime LastUpdate { get; set; }
}
