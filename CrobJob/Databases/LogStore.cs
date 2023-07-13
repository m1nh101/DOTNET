using StackExchange.Redis;

namespace CrobJob.Databases;

public class LogStore
{
    private readonly IDatabase _db;

    public LogStore(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();      
    }

    public void AddLog(string log) => _db.StringSet(DateTime.Now.ToString("yyyyMMddHHmm"), log);
}
