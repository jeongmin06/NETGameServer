using System.Net.Sockets;
using Server.Core.Memory;

namespace Server.Core.Network;

public sealed class SessionActor : Actor<SessionActor>
{
    private readonly Socket _socket;
    private readonly PacketDispatcher _dispatcher;

    public string SessionId { get; }

    public SessionActor(string sessionId, Socket socket, ActorChannel channel, PacketDispatcher dispatcher)
        : base(channel)
    {
        SessionId = sessionId;
        _socket = socket;
        _dispatcher = dispatcher;
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
                var pooled = PooledBuffer.RentAndCopy(bodySpan);
                var packet = new Packet(opcode, pooled);

                Post(new ActorMessage<SessionActor>(this, async (self) =>
                {
                    try
                    {
                        await self.HandlePacketAsync(packet, ct);   
                    }
                    finally
                    {
                        // 풀 반환 필수!
                        packet.Body.Dispose();
                    }
                }));
            }
        }

        _socket.Close();
    }

    private ValueTask HandlePacketAsync(Packet packet, CancellationToken ct)
    {
        return _dispatcher.DispatchAsync(this, packet, ct);
    }
}