namespace Server.Core.Test;

public class ActorChannelTest
{
    [Fact]
    public async Task Messages_Are_Processed_In_Order()
    {
        ActorThreadScheduler.ResetForTest();
        using var cts = new CancellationTokenSource();
        var worker = new ActorThread(0, cts.Token);
        var channel = new ActorChannel(cts.Token);

        var result = new List<int>();
        var lockObj = new object();

        for (int i = 0; i < 100; i++)
        {
            int capture = i;
            channel.Post(new TestMessage(async () =>
            {
                lock (lockObj)
                {
                    result.Add(capture);
                }
                await Task.CompletedTask; 
            }));
        }

        await Task.Delay(500);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(i, result[i]);
        }

        cts.Cancel();
        await worker.Completion;
    }

    private sealed class TestMessage(Func<ValueTask> action) : IActorMessage
    {
        public async ValueTask RunAsync()
        {
            await action();
        }
    }
}
