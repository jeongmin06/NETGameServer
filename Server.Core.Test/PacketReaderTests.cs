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

    [Fact]
    public void BodySplit_ShouldParse_WhenFullBodyArrives()
    {
        var reader = new PacketReader();
        var pkt = PacketTestHelper.MakePacket(7, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

        int first = 4 + 3;
        var m1 = reader.GetWriteMemory(first);
        pkt.AsSpan(0, first).CopyTo(m1.Span);
        reader.AdvanceWrite(first);

        Assert.False(reader.TryReadFrame(out _, out _));

        var m2 = reader.GetWriteMemory(pkt.Length - first);
        pkt.AsSpan(first).CopyTo(m2.Span);
        reader.AdvanceWrite(pkt.Length - first);

        Assert.True(reader.TryReadFrame(out var op, out var body));
        Assert.Equal((ushort)7, op);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, body.ToArray());
    }
}
