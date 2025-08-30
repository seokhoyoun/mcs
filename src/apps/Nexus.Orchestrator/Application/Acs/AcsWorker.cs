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
        private readonly AcsService _acsService;

        public AcsWorker(AcsService acsService)
        {
            _acsService = acsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _acsService.RunAsync(stoppingToken);
        }
    }
}
