using System;
using System.Buffers.Binary;

namespace Server.Core.Test;

public static class PacketTestHelper
{
    public static byte[] MakePacket(ushort opcode, byte[] body)
    {
        if (body.Length > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException();
        }

        var buf = new byte[4 + body.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(0, 2), opcode);
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(2, 2), (ushort)body.Length);
        body.CopyTo(buf.AsSpan(4));
        return buf;
    }
}
