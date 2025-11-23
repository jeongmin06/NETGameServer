namespace Server.Core;

public abstract class Actor<TSelf>(ActorChannel<ActorMessage<TSelf>> actorChannel) where TSelf : Actor<TSelf>, new()
{
    public void Post(ActorMessage<TSelf> message)
    {
        actorChannel.Post(message);
    }

    protected abstract ValueTask OnReceiveAsync(ActorMessage<TSelf> message);
}