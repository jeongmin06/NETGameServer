using System.Threading.Channels;
using Server.Core;

public sealed class ActorThreadPool
{
    private readonly List<ActorThread> _actorThreadPool = new();
    private readonly CancellationTokenSource _cts = new();

    public static ActorThreadPool Instance { get; } = new ActorThreadPool(30);

    private ActorThreadPool(int threadCount)
    {
        for (int i = 0; i < threadCount; i++)
        {
            var thread = new ActorThread(i, _cts.Token);
            _actorThreadPool.Add(thread);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        ActorThreadScheduler.Complete();

        await Task.WhenAll(_actorThreadPool.Select(t => t.Completion));
    }
}