using System.Threading.Channels;
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
                if (channel is null)
                {
                    break;
                }

                await channel.RunAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            
        }
        catch (ChannelClosedException)
        {
            
        }
    }

    public Task Completion => _workerTask;
}