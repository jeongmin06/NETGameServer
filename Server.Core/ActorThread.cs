
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Server.Core;

public class ActorThread
{
    private readonly CancellationToken _ct;
    private readonly BlockingCollection<ActorChannel> _pendingQueue = new();
    private readonly Thread _thread;
    
    public string Name { get; }

    public ActorThread(int index, CancellationToken ct)
    {
        _ct = ct;
        Name = $"ActorThread-{index}";
        _thread = new Thread(Run);
    }

    public async void Run()
    {
        foreach (var channel in _pendingQueue.GetConsumingEnumerable(_ct))
        {
            await channel.RunAsync();
        }
    }

    public void EnqueueChannel(ActorChannel channel)
    {
        _pendingQueue.TryAdd(channel);
    }
}