
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Server.Core;

public class ActorThread
{
    private readonly CancellationToken _ct;
    private readonly Task _workerTask;
    
    public string Name { get; }

    public ActorThread(int index, CancellationToken ct)
    {
        _ct = ct;
        Name = $"ActorThread-{index}";
        _workerTask = Task.Factory.StartNew(
            async () => await RunLoopAsync(ct),
            ct,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var channel = await ActorThreadScheduler.DequeueAsync(ct);
                await channel.RunAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    public Task Completion => _workerTask;
}