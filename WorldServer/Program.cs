using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Core.Network;

namespace WorldServer
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<PacketDispatcher>();
                    services.AddHostedService<WorldServerService>();
                });
        }

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            await host.RunAsync();
        }
    }
}
