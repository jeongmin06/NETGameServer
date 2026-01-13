using System;
using Server.Core.Network;

namespace Server.Core.Test;

public class PacketReaderTests
{
    [Fact]
    public void HeaderSplit_ShouldParse_WhenFullArrives()
    {
        var reader = new PacketReader();
        var pkt = PacketTestHelper.MakePacket(1, new byte[] { 10, 20, 30 });

        var m1 = reader.GetWriteMemory(1);
        pkt.AsSpan(0, 1).CopyTo(m1.Span);
        reader.AdvanceWrite(1);

        Assert.False(reader.TryReadFrame(out _, out _));

        var m2 = reader.GetWriteMemory(pkt.Length - 1);
        pkt.AsSpan(1).CopyTo(m2.Span);
        reader.AdvanceWrite(pkt.Length - 1);

        Assert.True(reader.TryReadFrame(out var op, out var body));
        Assert.Equal((ushort)1, op);
        Assert.Equal(new byte[] { 10, 20, 30 }, body.ToArray());
    }
}
