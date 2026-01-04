namespace Server.Core.Network;

public interface IPacketHandler
{
    ushort OpCode { get; }
    ValueTask HandleAsync(SessionActor session, Packet packet, CancellationToken ct);
}
