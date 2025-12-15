using System;
using System.Buffers.Binary;

namespace Server.Core.Network;

public sealed class PacketReader
{
    private const int HeaderSize = 4;

    private byte[] _buffer;
    private int _readPos;
    private int _writePos;

    public PacketReader(int initialSize = 64 * 1024)
    {
        _buffer = new byte[initialSize];
    }

    public Memory<byte> GetWriteMemory(int minSize)
    {
        EnsureCapacity(minSize);
        return _buffer.AsMemory(_writePos);
    }

    public void AdvanceWrite(int count) => _writePos += count;

    public bool TryReadFrame(out ushort opcode, out ReadOnlySpan<byte> body)
    {
        opcode = 0;
        body = default;

        int available = _writePos - _readPos;
        if (available < HeaderSize)
        {
            return false;
        }

        var span = _buffer.AsSpan(_readPos, available);

        opcode = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(0, 2));
        ushort bodyLen = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(2, 2));

        int packetSize = HeaderSize + bodyLen;
        if (available < packetSize)
        {
            return false;
        }

        body = span.Slice(HeaderSize, bodyLen);
        _readPos += packetSize;

        CompactIfNeeded();
        return true;
    }

    private void EnsureCapacity(int minSize)
    {
        int free = _buffer.Length - _writePos;
        if (free >= minSize)
        {
            return;
        }

        CompactIfNeeded(force: true);

        free = _buffer.Length - _writePos;
        if (free >= minSize)
        {
            return;
        }

        int newSize = Math.Max(_buffer.Length * 2, _writePos + minSize);
        Array.Resize(ref _buffer, newSize);
    }

    private void CompactIfNeeded(bool force = false)
    {
        if (!force && _readPos < 32 * 1024)
        {
            return;
        }

        int remaining = _writePos - _readPos;
        if (remaining > 0)
        {
            Buffer.BlockCopy(_buffer, _readPos, _buffer, 0, remaining);
        }
        _readPos = 0;
        _writePos = remaining;
    }
}
