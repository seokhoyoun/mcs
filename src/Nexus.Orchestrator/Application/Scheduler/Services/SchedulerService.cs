using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Core.Domain.Models.Plans;
using Nexus.Core.Domain.Models.Plans.Enums;
using Nexus.Core.Domain.Models.Plans.Interfaces;
using Nexus.Core.Domain.Models.Plans.Strategies;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Messaging;
using Nexus.Shared.Application.Interfaces;
using System.Collections.Generic;

namespace Nexus.Orchestrator.Application.Scheduler.Services
{
    public class SchedulerService
    {
        private readonly AreaService _areaService;

        public SchedulerService(AreaService areaService)
        {
            _areaService = areaService;
        }

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            await _areaService.InitializeAreaService();
        }

        public void CreatePlanGroup(Lot lot)
        {
            foreach (var lotStep in lot.LotSteps)
            {
                string id = Guid.NewGuid().ToString();
                string name = lotStep.Name;
                EPlanGroupType groupType = EPlanGroupType.StockerToArea; 

                var pg = new PlanGroup(id, name, groupType);

                foreach (var cassette in lotStep.Cassettes)
                {
                    Plan plan = new Plan("PID", $"{pg.Id}_{cassette.Id}");
                    PlanStep cassetteLoadStep = new PlanStep("ps", "ps", 1, EPlanStepAction.CassetteLoad, "");
                    PlanStep cassetteUnloadStep = new PlanStep("ps", "ps", 2, EPlanStepAction.CassetteUnload, "");

                    plan.PlanSteps.Add(cassetteLoadStep);
                    plan.PlanSteps.Add(cassetteUnloadStep);

                    pg.Plans.Add(plan);
                }

            }
        }

    }
}