using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;

namespace Server.Core.Network;

public static class PacketWriter
{
    public const int HeaderSize = 4;

    public static async ValueTask SendAsync(Socket socket, ushort opcode, ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        if (body.Length > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(body), "body length must be <= 65535");
        }

        byte[] header = ArrayPool<byte>.Shared.Rent(HeaderSize);
        try
        {
            var span = header.AsSpan(0, HeaderSize);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(0, 2), opcode);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(2, 2), (ushort)body.Length);

            var segments = new List<ArraySegment<byte>>(2)
            {
                new ArraySegment<byte>(header, 0, HeaderSize),
                body.Length == 0 ? default : new ArraySegment<byte>(body.ToArray(), 0, body.Length)
            };
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(header);
        }
    }
}
