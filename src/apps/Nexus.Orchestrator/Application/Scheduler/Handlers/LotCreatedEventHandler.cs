using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Orchestrator.Application.Scheduler.Services;
using Nexus.Core.Domain.Shared.Events;

namespace Nexus.Orchestrator.Application.Scheduler.Handlers
{
    public class LotCreatedEventHandler : IEventHandler<LotCreatedEvent>
    {
        private readonly SchedulerService _schedulerService;

        public LotCreatedEventHandler(SchedulerService schedulerService)
        {
            _schedulerService = schedulerService;
        }

        public async Task HandleAsync(LotCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            // Lot 정보를 조회하는 로직 필요 (예: LotRepository 등)
            // 아래는 예시로 Lot 객체를 직접 생성/조회했다고 가정
            // 실제로는 DI로 ILotRepository를 받아서 조회해야 함

            // 예시: Lot lot = await _lotRepository.GetByIdAsync(@event.LotId, cancellationToken);

            // 실제 Lot 조회 로직으로 대체 필요
            Lot? lot = await GetLotByIdAsync(@event.LotId, cancellationToken);
            if (lot == null)
            {
                return;
            }

            // PlanGroup 생성
            //_schedulerService.CreatePlanGroup(lot);
        }

        // 실제 환경에서는 Repository에서 Lot을 조회해야 함
        private async Task<Lot?> GetLotByIdAsync(string lotId, CancellationToken cancellationToken)
        {
            // TODO: DI로 ILotRepository 받아서 조회하도록 개선
            await Task.CompletedTask;
            return null;
        }
    }
}
