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
                _logger.LogInformation($"[plan] 메시지 수신: {message}");
                // TODO: plan 메시지 처리 로직 구현
            }, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}