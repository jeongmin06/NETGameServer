
using Server.Core;

public class ActorThread
{
    public async ValueTask RunAsync(ActorChannel actorChannel)
    {
        await actorChannel.RunAsync();
    }
}