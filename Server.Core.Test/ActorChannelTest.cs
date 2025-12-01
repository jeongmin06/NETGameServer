namespace Server.Core.Test;

public class ActorChannelTest
{
    [Fact]
    public async Task Messages_Are_Processed_In_Order()
    {
        using var cts = new CancellationTokenSource();
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

        _ = ActorThreadPool.Instance;

        await Task.Delay(500);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(i, result[i]);
        }
    }

    private sealed class TestMessage(Func<ValueTask> action) : IActorMessage
    {
        public async ValueTask RunAsync()
        {
            await action();
        }
    }
}
