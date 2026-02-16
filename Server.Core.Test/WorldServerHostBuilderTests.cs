using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Core.Network;
using WorldServer;

namespace Server.Core.Test;

public class WorldServerHostBuilderTests
{
    [Fact]
    public void CreateHostBuilder_ShouldRegisterWorldServerDependencies()
    {
        using var host = Program.CreateHostBuilder(Array.Empty<string>()).Build();

        var dispatcher = host.Services.GetService<PacketDispatcher>();
        var hostedServices = host.Services.GetServices<IHostedService>();

        Assert.NotNull(dispatcher);
        Assert.Contains(hostedServices, service => service is WorldServerService);
    }
}
