using CrobJob.Databases;
using Cronos;

namespace CrobJob.Crons;

public class CronosTimerJob : IHostedService, IDisposable
{
    private System.Timers.Timer? _timer;
    private readonly CronExpression _expression;
    private readonly TimeZoneInfo _timeZoneInfo;
    private readonly IServiceProvider _provider;

    public CronosTimerJob(IServiceProvider provider)
    {
        _expression = CronExpression.Parse("10 * * * * *", CronFormat.IncludeSeconds);
        _timeZoneInfo = TimeZoneInfo.Local;
        _provider = provider;
    }

    public void Dispose() => _timer?.Dispose();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);

        if(next.HasValue)
        {
            var delay = next.Value - DateTimeOffset.Now;

            if (delay.TotalMilliseconds <= 0)
                await StartAsync(cancellationToken);

            _timer = new System.Timers.Timer(delay.TotalMilliseconds);

            _timer.Elapsed += async (sender, arg) =>
            {
                _timer.Dispose();

                _timer = null;

                await LogToDatabaseJob();

                if (!cancellationToken.IsCancellationRequested)
                    await StartAsync(cancellationToken);
            };

            _timer.Start();
        }

        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task LogToDatabaseJob()
    {
        using var scope = _provider.CreateScope();

        var store = scope.ServiceProvider.GetRequiredService<LogStore>();

        store.AddLog("Cron start");

        return Task.CompletedTask;
    }
}
