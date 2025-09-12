using Microsoft.AspNetCore.SignalR;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Robots.DTO;
using Nexus.Orchestrator.Application.Hubs;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Models.Robots.Services;
using Nexus.Orchestrator.Application.Scheduler.Services;

namespace Nexus.Orchestrator.Application.Scheduler
{
    internal class SchedulerWorker : BackgroundService
    {
        private readonly ILogger<SchedulerWorker> _logger;
        private readonly IHubContext<RobotPositionHub> _hubContext;
        private readonly SchedulerService _schedulerService;
        private readonly IRobotRepository _robotRepository;

        public SchedulerWorker(ILogger<SchedulerWorker> logger,
                               IHubContext<RobotPositionHub> hubContext,
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

                    List<RobotDto> updates = new List<RobotDto>();
                    foreach (Robot robot in robots)
                    {
                        RobotDto dto = new RobotDto
                        {
                            Id = robot.Id,
                            Name = robot.Name,
                            RobotType = robot.RobotType.ToString(),
                            X = (int)robot.Position.X,
                            Y = (int)robot.Position.Y,
                            Z = (int)robot.Position.Z
                        };
                        updates.Add(dto);
                    }

                    await _hubContext.Clients.All.SendAsync("ReceiveRobotPosition", updates, stoppingToken);
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
