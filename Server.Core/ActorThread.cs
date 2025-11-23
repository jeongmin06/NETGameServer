
using Server.Core;

public class ActorThread
{
    public async ValueTask RunAsync(ActorChannel<IActorMessage> actorChannel)
    {
        await actorChannel.RunAsync();
    }
}