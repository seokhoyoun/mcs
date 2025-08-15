using Nexus.Core.Domain.Models.Areas; 
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Messaging;
using Nexus.Scheduler.Application.Services;
using StackExchange.Redis;

namespace Nexus.Scheduler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly SchedulerService _schedulerService;

        public Worker(ILogger<Worker> logger, SchedulerService schedulerService)
        {
            _logger = logger;
            _schedulerService = schedulerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _schedulerService.RunAsync(stoppingToken);
        }
    }
}