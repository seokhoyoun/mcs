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
            _logger.LogInformation("Scheduler Worker running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                await _schedulerService.RunAsync(stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}