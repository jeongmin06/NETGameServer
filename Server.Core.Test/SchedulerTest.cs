using Server.Core;
using Xunit;

namespace Server.Core.Test;

public class SchedulerTest
{
    [Fact]
    public async Task Scheduler_Processes_Only_Once_When_Scheduled_Many_Times()
    {
        ActorThreadScheduler.ResetForTest();
        using var cts = new CancellationTokenSource();
        var worker = new ActorThread(0, cts.Token);
        var channel = new ActorChannel(cts.Token);

        int runCount = 0;

        channel.Post(new TestMessage(async () =>
        {
            Interlocked.Increment(ref runCount);
            await ValueTask.CompletedTask;
        }));

        channel.Post(new TestMessage(async () =>
        {
            Interlocked.Increment(ref runCount);
            await ValueTask.CompletedTask;
        }));

        await Task.Delay(500);

        Assert.Equal(2, Volatile.Read(ref runCount));

        cts.Cancel();
        await worker.Completion;
    }

    private sealed class TestMessage(Func<ValueTask> action) : IActorMessage
    {
        public ValueTask RunAsync() => action();
    }
}
