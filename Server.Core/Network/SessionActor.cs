using System.Net.Sockets;

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
        var reader = new PacketReader();

        while (!ct.IsCancellationRequested)
        {
            var mem = reader.GetWriteMemory(4096);
            int receiveByte = await _socket.ReceiveAsync(mem, SocketFlags.None, ct);
            if (receiveByte <= 0)
            {
                break;
            }

            reader.AdvanceWrite(receiveByte);
            
            while (reader.TryReadFrame(out var opcode, out var bodySpan))
            {
                byte[] body = bodySpan.ToArray();

                Post(new ActorMessage<SessionActor>(this, async (self) =>
                {
                    await self.HandlePacketAsync(opcode, body, ct);
                }));
            }
        }

        _socket.Close();
    }

    private async ValueTask HandlePacketAsync(ushort opcode, byte[] body, CancellationToken ct)
    {
        if (opcode == 1)    //ex. opcode 1 : Ping
        {
            await PacketWriter.SendAsync(_socket, 2, ReadOnlyMemory<byte>.Empty, ct);
        }
    }
}