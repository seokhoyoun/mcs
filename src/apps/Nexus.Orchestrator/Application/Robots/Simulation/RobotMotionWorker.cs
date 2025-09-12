using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Orchestrator.Application.Robots.Simulation
{
    internal class RobotMotionWorker : BackgroundService
    {
        private readonly ILogger<RobotMotionWorker> _logger;
        private readonly IRobotRepository _robotRepository;
        private readonly RobotMotionService _motionService;

        public RobotMotionWorker(ILogger<RobotMotionWorker> logger,
                                 IRobotRepository robotRepository,
                                 RobotMotionService motionService)
        {
            _logger = logger;
            _robotRepository = robotRepository;
            _motionService = motionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int intervalMs = 100; // 10 Hz update
            double deltaSeconds = (double)intervalMs / 1000.0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    IReadOnlyList<RobotMotionState> motions = _motionService.Snapshot();

                    foreach (RobotMotionState motion in motions)
                    {
                        Position? current = await _robotRepository.GetPositionAsync(motion.RobotId, stoppingToken);
                        if (current == null)
                        {
                            continue;
                        }

                        double dx = motion.TargetX - current.X;
                        double dy = motion.TargetY - current.Y;
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        double step = motion.Speed * deltaSeconds;

                        if (distance <= step || distance <= 0.0001)
                        {
                            Position finalPos = new Position((uint)Math.Round(motion.TargetX), (uint)Math.Round(motion.TargetY), current.Z);
                            bool ok = await _robotRepository.UpdatePositionAsync(motion.RobotId, finalPos, stoppingToken);
                            if (ok)
                            {
                                _motionService.Complete(motion.RobotId);
                            }
                            continue;
                        }

                        double ux = dx / distance;
                        double uy = dy / distance;
                        double nx = (double)current.X + ux * step;
                        double ny = (double)current.Y + uy * step;

                        if (nx < 0) { nx = 0; }
                        if (ny < 0) { ny = 0; }

                        Position nextPos = new Position((uint)Math.Round(nx), (uint)Math.Round(ny), current.Z);
                        await _robotRepository.UpdatePositionAsync(motion.RobotId, nextPos, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RobotMotionWorker");
                }

                try
                {
                    await Task.Delay(intervalMs, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}

