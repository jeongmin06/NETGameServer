using Server.Core;
using Xunit;

namespace Server.Core.Test;

public class SchedulerTest
{
    [Fact]
    public async Task Scheduler_Processes_Only_Once_When_Scheduled_Many_Times()
    {
        using var cts = new CancellationTokenSource();
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

        _ = ActorThreadPool.Instance;

        await Task.Delay(500);

        Assert.Equal(2, Volatile.Read(ref runCount));
    }

    private sealed class TestMessage(Func<ValueTask> action) : IActorMessage
    {
        public ValueTask RunAsync() => action();
    }
}