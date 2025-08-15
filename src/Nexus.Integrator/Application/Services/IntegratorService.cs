using Nexus.Core.Messaging;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Integrator.Application.Services
{
    public class IntegratorService
    {
        private readonly IMessageSubscriber _subscriber;
        private readonly ILogger<IntegratorService> _logger;

        public IntegratorService(IMessageSubscriber subscriber, ILogger<IntegratorService> logger)
        {
            _subscriber = subscriber;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            await _subscriber.SubscribeAsync("plan", async message =>
            {
                _logger.LogInformation($"[plan] �޽��� ����: {message}");
                // TODO: plan �޽��� ó�� ���� ����
            }, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}