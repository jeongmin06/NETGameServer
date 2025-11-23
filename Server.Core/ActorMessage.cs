namespace Server.Core;

public class ActorMessage<TSelf>(TSelf actor, Action<TSelf> action) : IActorMessage where TSelf : class
{
    public void Run()
    {
        action.Invoke(actor);
    }
}
