using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Shared.Application.Interfaces;
using Nexus.Orchestrator.Application.Scheduler.Services;

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
            // Lot ������ ��ȸ�ϴ� ���� �ʿ� (��: LotRepository ��)
            // �Ʒ��� ���÷� Lot ��ü�� ���� ����/��ȸ�ߴٰ� ����
            // �����δ� DI�� ILotRepository�� �޾Ƽ� ��ȸ�ؾ� ��

            // ����: Lot lot = await _lotRepository.GetByIdAsync(@event.LotId, cancellationToken);

            // ���� Lot ��ȸ �������� ��ü �ʿ�
            Lot? lot = await GetLotByIdAsync(@event.LotId, cancellationToken);
            if (lot == null)
                return;

            // PlanGroup ����
            //_schedulerService.CreatePlanGroup(lot);
        }

        // ���� ȯ�濡���� Repository���� Lot�� ��ȸ�ؾ� ��
        private async Task<Lot?> GetLotByIdAsync(string lotId, CancellationToken cancellationToken)
        {
            // TODO: DI�� ILotRepository �޾Ƽ� ��ȸ�ϵ��� ����
            await Task.CompletedTask;
            return null;
        }
    }
}