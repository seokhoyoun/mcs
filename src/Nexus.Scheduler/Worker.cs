using Nexus.Core.Domain.Models.Locations;
using StackExchange.Redis;

namespace Nexus.Scheduler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly LocationService _locationService;

        public Worker(ILogger<Worker> logger, LocationService locationService)
        {
            _logger = logger;
            _locationService = locationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _locationService.InitializeAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}