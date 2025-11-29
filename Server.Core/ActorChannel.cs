using System.Threading.Channels;

namespace Server.Core;

public sealed class ActorChannel
{
    private readonly Channel<IActorMessage> _channel;
    private readonly CancellationToken _token;

    private bool _scheduled;

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
        _channel.Writer.WriteAsync(message, _token);

        if (Interlocked.Exchange(ref _scheduled, true) == false)
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

            if (!_channel.Reader.TryPeek(out _))
            {
                if (Interlocked.Exchange(ref _scheduled, false) == false)
                {
                    ActorThreadScheduler.Schedule(this);
                }
            }
            return;
        }

        await foreach (var message in _channel.Reader.ReadAllAsync(_token))
        {
            await message.RunAsync();
        }
    }
}