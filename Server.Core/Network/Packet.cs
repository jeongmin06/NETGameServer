using Server.Core.Memory;

namespace Server.Core.Network;

public readonly struct Packet
{
    public readonly ushort OpCode;
    public readonly PooledBuffer Body;

    public Packet(ushort opCode, PooledBuffer body)
    {
        OpCode = opCode;
        Body = body;
    }
}
