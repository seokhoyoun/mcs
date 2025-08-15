using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Integrator.Application.Services;

namespace Nexus.Integrator
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IntegratorService _integratorService;

        public Worker(ILogger<Worker> logger, IntegratorService integratorService)
        {
            _logger = logger;
            _integratorService = integratorService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _integratorService.RunAsync(stoppingToken);
        }
    }
}
