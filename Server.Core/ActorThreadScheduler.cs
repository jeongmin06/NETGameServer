using System.Threading.Channels;

namespace Server.Core;

public static class ActorThreadScheduler
{
    private static readonly Channel<ActorChannel> _readyChannels = Channel.CreateUnbounded<ActorChannel>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false
    });

    public static void Schedule(ActorChannel channel)
    {
        _readyChannels.Writer.TryWrite(channel);
    }

    internal static ValueTask<ActorChannel> DequeueAsync(CancellationToken ct)
    {
        return _readyChannels.Reader.ReadAsync(ct);
    }

    public static void Complete()
    {
        _readyChannels.Writer.TryComplete();
    }
}