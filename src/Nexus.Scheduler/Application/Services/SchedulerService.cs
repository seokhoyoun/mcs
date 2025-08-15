using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Core.Domain.Models.Plans;
using Nexus.Core.Domain.Models.Plans.Interfaces;
using Nexus.Core.Domain.Models.Plans.Strategies;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Messaging;
using Nexus.Shared.Application.Interfaces;
using System.Collections.Generic;

namespace Nexus.Scheduler.Application.Services
{
    public class SchedulerService
    {
        private readonly AreaService _areaService;
        private readonly IMessageSubscriber _subscriber;
        private readonly LocationStatusChangedMessageHandler _locationStatusChangedHandler;

        public SchedulerService(
            AreaService areaService,
            IMessageSubscriber subscriber,
            LocationStatusChangedMessageHandler locationStatusChangedHandler)
        {
            _areaService = areaService;
            _subscriber = subscriber;
            _locationStatusChangedHandler = locationStatusChangedHandler;
        }

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            await _areaService.InitializeAreaService();

            // Redis 구독 설정
            await _subscriber.SubscribeAsync("location", async message =>
            {
                await _locationStatusChangedHandler.HandleAsync(message, stoppingToken);
            }, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public void CreatePlanGroupForLot(Lot lot)
        {
            // 실제 로직: lot의 정보(Chipset, Qty, Line 등)를 분석하여
            // 필요한 운반 Plan 목록을 생성하는 복잡한 알고리즘이 들어갈 위치입니다.
            // 여기서는 예시로 간단한 Plan을 생성합니다.
            var plans = new List<Plan>();
            

            // Lot의 특성에 따라 다른 전략(Parallel, Sequential 등)을 선택할 수 있습니다.
            IPlanExecutionStrategy executionStrategy = new ParallelPlanStrategy();

            var planGroup = new PlanGroup(id: $"PlanGroup-{lot.Id}", name: $"PlanGroup for Lot {lot.Id}", executionStrategy: executionStrategy, plans: plans);

        }

        // Location 관련 작업 예시
        public void LoadItem(string locationId, ITransportable item)
        {

        }

    }
}