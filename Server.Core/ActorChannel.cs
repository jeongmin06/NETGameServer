using System.Threading.Channels;

namespace Server.Core;

public sealed class ActorChannel
{
    private readonly Channel<IActorMessage> _channel;
    private readonly CancellationToken _token;

    private int _scheduled;

    public ActorChannel(CancellationToken token)
    {
        _token = token;
        _channel = Channel.CreateUnbounded<IActorMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public void Post(IActorMessage message)
    {
        if (!_channel.Writer.TryWrite(message))
        {
            return;
        }

        if (Interlocked.Exchange(ref _scheduled, 1) == 0)
        {
            ActorThreadScheduler.Schedule(this);
        }
    }

    internal async ValueTask RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && !_token.IsCancellationRequested)
        {
            while (_channel.Reader.TryRead(out var msg))
            {
                await msg.RunAsync();
            }

            if (_channel.Reader.TryPeek(out _))
            {
                ActorThreadScheduler.Schedule(this);
                return;
            }

            Interlocked.Exchange(ref _scheduled, 0);

            if (_channel.Reader.TryPeek(out _))
            {
                if (Interlocked.Exchange(ref _scheduled, 1) == 0)
                {
                    ActorThreadScheduler.Schedule(this);
                }
            }

            return;
        }
    }
}