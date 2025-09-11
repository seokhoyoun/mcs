using Microsoft.AspNetCore.SignalR;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.Hubs;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Models.Robots.Services;
using Nexus.Orchestrator.Application.Scheduler.Services;

namespace Nexus.Orchestrator.Application.Scheduler
{
    internal class SchedulerWorker : BackgroundService
    {
        private readonly ILogger<SchedulerWorker> _logger;
        private readonly IHubContext<RobotPositionMessageHub> _hubContext;
        private readonly SchedulerService _schedulerService;
        private readonly IRobotRepository _robotRepository;

        public SchedulerWorker(ILogger<SchedulerWorker> logger,
                               IHubContext<RobotPositionMessageHub> hubContext,
                               SchedulerService schedulerService,
                               IRobotRepository robotRepository)
        {
            _logger = logger;
            _hubContext = hubContext;
            _schedulerService = schedulerService;
            _robotRepository = robotRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler Worker starting at: {time}", DateTimeOffset.Now);

            await _schedulerService.StartAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    IReadOnlyList<Robot> robots = await _robotRepository.GetAllAsync(stoppingToken); 
                    await _hubContext.Clients.All.SendAsync("ReceiveRobotPosition", robots, stoppingToken);
                    await Task.Delay(200, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ProcessPendingLotsAsync");
                    await Task.Delay(5000, stoppingToken); // 에러 시 5초 대기
                }
            }
        }

       
    }
}
