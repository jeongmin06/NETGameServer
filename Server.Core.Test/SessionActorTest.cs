using System.Text;

namespace Server.Core.Test;

public class SessionActorTest
{
    public sealed class FakeSocket
    {
        public Queue<byte[]> Received = new();
        public List<string> Sent = new();

        public ValueTask<int> FakeReceiveAsync(byte[] buffer)
        {
            if (Received.Count == 0)
                return ValueTask.FromResult(0);

            var data = Received.Dequeue();
            Array.Copy(data, buffer, data.Length);
            return ValueTask.FromResult(data.Length);
        }

        public ValueTask FakeSendAsync(byte[] buffer)
        {
            Sent.Add(Encoding.UTF8.GetString(buffer));
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task SessionActor_Processes_Received_Message()
    {
        using var cts = new CancellationTokenSource();
        var channel = new ActorChannel(cts.Token);

        var fake = new FakeSocket();
        fake.Received.Enqueue(Encoding.UTF8.GetBytes("hello"));
        fake.Received.Enqueue(Encoding.UTF8.GetBytes("world"));

        var actor = new TestSessionActor("session-1", fake, channel);

        _ = ActorThreadPool.Instance;
        
        await actor.StartFakeReceiveLoop(cts.Token);

        await Task.Delay(200);

        Assert.Equal(2, actor.Received.Count);
        Assert.Equal("hello", actor.Received[0]);
        Assert.Equal("world", actor.Received[1]);
    }

    private sealed class TestSessionActor : Actor<TestSessionActor>
    {
        public FakeSocket Socket { get; }
        public List<string> Received { get; } = new();

        public TestSessionActor(string id, FakeSocket socket, ActorChannel channel)
            : base(channel)
        {
            Socket = socket;
        }

        public async Task StartFakeReceiveLoop(CancellationToken ct)
        {
            byte[] buffer = new byte[1024];

            while (!ct.IsCancellationRequested)
            {
                var len = await Socket.FakeReceiveAsync(buffer);
                if (len <= 0)
                    break;

                string text = Encoding.UTF8.GetString(buffer, 0, len);

                Post(new ActorMessage<TestSessionActor>(this, async (self) =>
                {
                    self.Received.Add(text);
                    await ValueTask.CompletedTask;
                }));
            }
        }
    }
}