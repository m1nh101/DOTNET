using CrobJob.Databases;
using Quartz;

namespace CrobJob.Crons;

public class QuartzJob : IJob
{
    private readonly LogStore _store;

    public QuartzJob(LogStore store)
    {
        _store = store;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _store.AddLog($"Run at {DateTime.Now:yyyy/MM/dd - HH:mm}");

        return Task.CompletedTask;
    }
}
