using System.Threading.Channels;

namespace Server.Core;

public sealed class ActorChannel<TMessage> where TMessage : IActorMessage
{
    private readonly Channel<TMessage> _channel;
    private readonly CancellationToken _token;

    public ActorChannel(CancellationToken token)
    {
        _token = token;
        _channel = Channel.CreateUnbounded<TMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public void Post(TMessage message)
    {
        _channel.Writer.WriteAsync(message, _token);
    }

    public async ValueTask RunAsync()
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(_token))
        {
            await message.RunAsync();
        }
    }
}