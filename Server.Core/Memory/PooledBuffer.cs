using System.Buffers;

namespace Server.Core.Memory;

public sealed class PooledBuffer : IDisposable
{
    private byte[]? _buffer;

    public int Length { get; private set; }

    public ReadOnlyMemory<byte> Memory => _buffer is null ? ReadOnlyMemory<byte>.Empty : new ReadOnlyMemory<byte>(_buffer, 0, Length);
    public ReadOnlySpan<byte> Span => _buffer is null ? ReadOnlySpan<byte>.Empty : new ReadOnlySpan<byte>(_buffer, 0, Length);

    private PooledBuffer(byte[] buffer, int length)
    {
        _buffer = buffer;
        Length = length;
    }

    public static PooledBuffer RentAndCopy(ReadOnlySpan<byte> src)
    {
        var arr = ArrayPool<byte>.Shared.Rent(src.Length);
        src.CopyTo(arr);
        return new PooledBuffer(arr, src.Length);    
    }

    public void Dispose()
    {
        var arr = _buffer;
        _buffer = null;
        Length = 0;

        if (arr != null)
        {
            ArrayPool<byte>.Shared.Return(arr);
        }
    }
}
