using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Models.Lots.Events;
using Nexus.Core.Domain.Models.Lots.Interfaces;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Gateway.Services.Commands;
using Nexus.Gateway.Services.Interfaces;
using Nexus.Shared.Application.Interfaces;

namespace Nexus.Gateway.Services
{
    public class LotCreationService : ILotCreationService
    {
        private readonly ILotRepository _lotRepository;
        private readonly IEventPublisher _eventPublisher;

        public LotCreationService(
            ILotRepository lotRepository,
            IEventPublisher eventPublisher)
        {
            _lotRepository = lotRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<string> CreateLotAsync(CreateLotCommand command, CancellationToken cancellationToken = default)
        {
            // Lot ����
            var lot = new Lot(
                id: command.Id,
                name: command.Name,
                status: ELotStatus.Waiting,
                priority: command.Priority,
                receivedTime: DateTime.UtcNow,
                purpose: command.Purpose,
                evalNo: command.EvalNo,
                partNo: command.PartNo ,
                qty: command.Qty,
                option: command.Option,
                line: command.Line,
                cassetteIds: command.CassetteIds
            );

            // LotStep ���� (��û�� ���Ե� ���)
            if (command.Steps != null && command.Steps.Any())
            {
                foreach (CreateLotStepCommand stepCommand in command.Steps)
                {
                    string stepId = $"{lot.Id}_{stepCommand.No:D2}";

                    var lotStep = new LotStep(
                        id: stepId,
                        lotId: lot.Id,
                        name: stepId,
                        loadingType: stepCommand.LoadingType,
                        dpcType: stepCommand.DpcType,
                        chipset: stepCommand.Chipset,
                        pgm: stepCommand.PGM,
                        planPercent: stepCommand.PlanPercent,
                        status: ELotStatus.Waiting
                    );

                    // LotStep�� Lot�� �߰�
                    lot.LotSteps.Add(lotStep);

                    //await _lotRepository.AddLotStepAsync(lot.Id, lotStep, cancellationToken);
                }
            }

            // Lot ���� (Redis lot:{lotId})
            await _lotRepository.AddAsync(lot, cancellationToken);

            // LotCreatedEvent�� �����Ͽ� Orchestrator�� �޽��� ����
            var lotCreatedEvent = new LotCreatedEvent(lot.Id);
            await _eventPublisher.PublishAsync(lotCreatedEvent, cancellationToken);

            return lot.Id;
        }
    }
}