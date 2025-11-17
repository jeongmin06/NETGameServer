using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorldServer
{
    public class WorldServerService : BackgroundService
    {
        private readonly ILogger<WorldServerService> _logger;
        private readonly TcpListener _listener;

        public WorldServerService(ILogger<WorldServerService> logger)
        {
            _logger = logger;

            _listener = new TcpListener(IPAddress.Any, 5100);
        }

        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}