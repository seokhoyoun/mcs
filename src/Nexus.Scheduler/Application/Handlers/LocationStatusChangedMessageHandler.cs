using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Messaging;
using System.Threading;
using System.Threading.Tasks;


public class LocationStatusChangedMessageHandler : IMessageHandler<string>
{
    private readonly LocationService _locationService;
    private readonly ILogger<LocationStatusChangedMessageHandler> _logger;

    public LocationStatusChangedMessageHandler(LocationService locationService, ILogger<LocationStatusChangedMessageHandler> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    public async Task HandleAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Location 상태 변경 메시지 수신: {id}");
        _locationService.UpdateLocation(id);
        await Task.CompletedTask;
    }
}