using Nexus.Core.Domain.Models.Areas.Services;
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

        public void CreatePlanGroup(Lot lot)
        {
            

        }

    }
}