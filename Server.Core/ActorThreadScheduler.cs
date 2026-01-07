using System.Threading.Channels;

namespace Server.Core;

public static class ActorThreadScheduler
{
    private static Channel<ActorChannel> _readyChannels = CreateChannel();
    private static Channel<ActorChannel> CreateChannel() => Channel.CreateUnbounded<ActorChannel>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false
    });

    public static void Schedule(ActorChannel channel)
    {
        _readyChannels.Writer.TryWrite(channel);
    }

    internal static async ValueTask<ActorChannel?> DequeueAsync(CancellationToken ct)
    {
        try
        {
            return await _readyChannels.Reader.ReadAsync(ct);   
        }
        catch (ChannelClosedException)
        {
            return null;
        }
    }

    public static void Complete()
    {
        _readyChannels.Writer.TryComplete();
    }

    internal static void ResetForTest()
    {
        _readyChannels = CreateChannel();
    }
}