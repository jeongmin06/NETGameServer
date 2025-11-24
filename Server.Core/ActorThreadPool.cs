using System.Threading.Channels;
using System.Threading.Tasks;
using Server.Core;

public sealed class ActorThreadPool
{
    private readonly Channel<ActorThread> _actorThreadPool;
    private readonly Channel<ActorChannel> _readyChannelQueue;

    public ActorThreadPool(int threadCount)
    {
        _actorThreadPool = Channel.CreateBounded<ActorThread>(threadCount);
        _readyChannelQueue = Channel.CreateUnbounded<ActorChannel>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
    }

    public async Task AddReadyChannel(ActorChannel actorChannel)
    {
        await _readyChannelQueue.Writer.WriteAsync(actorChannel);
    }

    public async Task RunProcessing()
    {
        await foreach (var channel in _readyChannelQueue.Reader.ReadAllAsync())
        {
            var actorThread = await _actorThreadPool.Reader.ReadAsync();
            await actorThread.RunAsync(channel);
            await _actorThreadPool.Writer.WriteAsync(actorThread);
        }
    }
}