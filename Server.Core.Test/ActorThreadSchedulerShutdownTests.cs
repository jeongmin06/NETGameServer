using System;

namespace Server.Core.Test;

public class ActorThreadSchedulerShutdownTests
{
    [Fact]
    public async Task Complete_Should_Stop_Worker()
    {
        ActorThreadScheduler.ResetForTest();

        using var cts = new CancellationTokenSource();
        var worker = new ActorThread(0, cts.Token);

        ActorThreadScheduler.Complete();

        var finished = await Task.WhenAny(worker.Completion, Task.Delay(1000));
        Assert.Same(worker.Completion, finished);

        Assert.False(worker.Completion.IsFaulted);
    }
}
