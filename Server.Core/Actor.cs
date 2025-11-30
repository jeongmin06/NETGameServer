namespace Server.Core;

public abstract class Actor<TSelf>(ActorChannel actorChannel) where TSelf : Actor<TSelf>
{
    public void Post(ActorMessage<TSelf> message)
    {
        actorChannel.Post(message);
    }
}