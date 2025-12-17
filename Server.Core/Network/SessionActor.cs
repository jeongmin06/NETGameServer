using System.Net.Sockets;
using System.Text;

namespace Server.Core.Network;

public sealed class SessionActor : Actor<SessionActor>
{
    private readonly Socket _socket;

    public string SessionId { get; }

    public SessionActor(string sessionId, Socket socket, ActorChannel channel)
        : base(channel)
    {
        SessionId = sessionId;
        _socket = socket;
    }

    public async Task StartReceiveLoop(CancellationToken ct)
    {
        byte[] buffer = new byte[1024];

        while (!ct.IsCancellationRequested)
        {
            var received = await _socket.ReceiveAsync(buffer, SocketFlags.None, ct);

            if (received <= 0)
            {
                // Need Logging
                break;
            }

            string text = Encoding.UTF8.GetString(buffer, 0, received);
            var message = new ActorMessage<SessionActor>(this, async (actor) =>
            {
               await actor.OnMessageReceived(text); 
            });
            Post(message);
        }

        _socket.Close();
    }

    public ValueTask OnMessageReceived(string msg)
    {
        Console.WriteLine($"[Session {SessionId}] RECV: {msg}");
        return ValueTask.CompletedTask;
    }

    public ValueTask<int> SendAsync(string msg, CancellationToken ct)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(msg);
        return _socket.SendAsync(bytes, SocketFlags.None, ct);
    }
}