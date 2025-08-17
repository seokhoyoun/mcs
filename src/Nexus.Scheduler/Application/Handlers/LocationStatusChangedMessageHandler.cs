using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Locations.Service;
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
        _logger.LogInformation($"Location ���� ���� �޽��� ����: {id}");
        await _locationService.RefreshLocationStateAsync(id);
        await Task.CompletedTask;
    }
}