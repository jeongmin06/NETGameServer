namespace Server.Core.Network;

public sealed class PacketDispatcher
{
    private readonly Dictionary<ushort, IPacketHandler> _handlerDic = new();

    public void Register(IPacketHandler handler) => _handlerDic[handler.OpCode] = handler;

    public ValueTask DispatchAsync(SessionActor session, Packet packet, CancellationToken ct)
    {
        if (_handlerDic.TryGetValue(packet.OpCode, out var handler))
        {
            return handler.HandleAsync(session, packet, ct);
        }

        return ValueTask.CompletedTask;
    }
}
