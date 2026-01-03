using System.Net.Sockets;
using Server.Core.Memory;

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

    private async ValueTask HandlePacketAsync(ushort opcode, byte[] body, CancellationToken ct)
    {
        if (opcode == 1)    //ex. opcode 1 : Ping
        {
            await PacketWriter.SendAsync(_socket, 2, ReadOnlyMemory<byte>.Empty, ct);
        }
    }

    private async ValueTask HandlePacketAsync(Packet packet, CancellationToken ct)
    {
        if (packet.OpCode == 1)
        {
            await PacketWriter.SendAsync(_socket, 2, ReadOnlyMemory<byte>.Empty, ct);
        }
    }
}