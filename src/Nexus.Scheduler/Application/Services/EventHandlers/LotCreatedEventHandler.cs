using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Core.Domain.Models.Plans.Events;
using Nexus.Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Scheduler.Application.Services.EventHandlers
{
    internal class LotCreatedEventHandler : IEventHandler<LotCreatedEvent>
    {
        private readonly SchedulerService _schedulerService;
        private readonly IEventPublisher _eventPublisher;
        // Lot 정보에 접근하기 위한 Repository가 있다면 추가
        // private readonly ILotRepository _lotRepository;

        public LotCreatedEventHandler(SchedulerService planningService, IEventPublisher eventPublisher)
        {
            _schedulerService = planningService;
            _eventPublisher = eventPublisher;
        }

        public async Task HandleAsync(LotCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            // 이 예시에서는 가상의 Lot 객체를 사용합니다.
            var lot = new Nexus.Core.Domain.Models.Lots.Lot { Id = @event.LotId, Name = "New Lot" };
            _schedulerService.CreatePlanGroupForLot(lot);

            var lotPlanAssignedEvent = new LotPlanAssignedEvent(lot.Id);
            await _eventPublisher.PublishAsync(lotPlanAssignedEvent, cancellationToken);
        }
    }
}
