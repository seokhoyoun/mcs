using Nexus.Orchestrator.Application.Acs.Services;
using Nexus.Orchestrator.Application.Scheduler;
using Nexus.Orchestrator.Application.Scheduler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Orchestrator.Application.Acs
{
    internal class AcsWorker : BackgroundService
    {
        private readonly ILogger<AcsWorker> _logger;

        public AcsWorker(ILogger<AcsWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1초마다 체크
                    await Task.Delay(1000, stoppingToken);
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
