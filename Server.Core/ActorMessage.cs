namespace Server.Core;

public class ActorMessage<TSelf>(TSelf actor, Func<TSelf, ValueTask> action) : IActorMessage where TSelf : class
{
    public async ValueTask RunAsync()
    {
        await action(actor);
    }
}
