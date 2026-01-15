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

    [Fact]
    public void MultiplePacketsInOneReceive_ShouldParseAll()
    {
        var reader = new PacketReader();

        var p1 = PacketTestHelper.MakePacket(1, new byte[] {1});
        var p2 = PacketTestHelper.MakePacket(2, new byte[] {2, 2});
        var p3 = PacketTestHelper.MakePacket(3, Array.Empty<byte>());

        var all = p1.Concat(p2).Concat(p3).ToArray();

        var m = reader.GetWriteMemory(all.Length);
        all.CopyTo(m.Span);
        reader.AdvanceWrite(all.Length);

        Assert.True(reader.TryReadFrame(out var op1, out var b1));
        Assert.True(reader.TryReadFrame(out var op2, out var b2));
        Assert.True(reader.TryReadFrame(out var op3, out var b3));
        Assert.False(reader.TryReadFrame(out _, out _));

        Assert.Equal((ushort)1, op1);
        Assert.Equal((ushort)2, op2);
        Assert.Equal((ushort)3, op3);
        Assert.Equal(new byte[] { 1 }, b1.ToArray());
        Assert.Equal(new byte[] { 2, 2 }, b2.ToArray());
        Assert.Empty(b3.ToArray());
    }
}
