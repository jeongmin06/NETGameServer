using System.Net;
using System.Net.Sockets;
using Server.Core;
using Server.Core.Network;

public sealed class TcpServer(ActorChannel channel, CancellationToken ct)
{
    public async Task StartAsync(int port)
    {
        var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, port));
        listener.Listen();

        Console.WriteLine($"TCP Server started on {port}");

        while (!ct.IsCancellationRequested)
        {
            var socket = await listener.AcceptAsync(ct);
            string sessionId = Guid.NewGuid().ToString("N");

            var sessionActor = new SessionActor(sessionId, socket, channel, new PacketDispatcher());

            _ = sessionActor.StartReceiveLoop(ct);
        }
    }
}