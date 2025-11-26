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
            _ = ActorThreadPool.Instance.AddReadyChannel(this);
        }
    }

    public async ValueTask RunAsync()
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(_token))
        {
            await message.RunAsync();
        }
    }
}