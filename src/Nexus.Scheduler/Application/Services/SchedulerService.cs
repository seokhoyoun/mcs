using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Shared.Application.Interfaces;
using System.Collections.Generic;

internal class SchedulerService : IEventHandler<LotCreatedEvent>
{
    private readonly LocationService _locationService;
    private readonly IEventPublisher _eventPublisher;

    public SchedulerService(LocationService locationService, IEventPublisher eventPublisher)
    {
        _locationService = locationService;
        _eventPublisher = eventPublisher;
    }

    // Lot 도착 처리 및 이벤트 발행
    public async Task HandleAsync(LotCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        // 1. Lot 생성에 따른 스케줄링/할당 로직 작성
        // 예시: Lot을 적절한 Location에 할당
        // await _locationService.AssignLotAsync(@event.LotId, cancellationToken);

        // 2. 필요시 추가 비즈니스 로직 수행
        // 예: 상태 변경, 알림 발행 등

        // 실제 구현은 비즈니스 요구사항에 맞게 작성하세요.
    }

 

    // Location 관련 작업 예시
    public void LoadItem(string locationId, ITransportable item)
    {
       
    }

 
}