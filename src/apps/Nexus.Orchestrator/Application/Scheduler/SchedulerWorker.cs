using Nexus.Orchestrator.Application.Scheduler.Services;

namespace Nexus.Orchestrator.Application.Scheduler
{
    internal class SchedulerWorker : BackgroundService
    {
        private readonly ILogger<SchedulerWorker> _logger;
        private readonly SchedulerService _schedulerService;

        public SchedulerWorker(ILogger<SchedulerWorker> logger, SchedulerService schedulerService)
        {
            _logger = logger;
            _schedulerService = schedulerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler Worker starting at: {time}", DateTimeOffset.Now);

            await _schedulerService.StartAsync(stoppingToken);

        }

       
    }
}