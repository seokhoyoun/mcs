using Nexus.Core.Interfaces;
using Nexus.Core.Models;
using Nexus.Core.Services;
using System.Collections.Generic;

internal class SchedulerService
{
    // 모든 Location 인스턴스를 관리하는 컬렉션
    private readonly Dictionary<string, Location<ITransportable>> _locations = new();

    private readonly LocationService _locationService;

    public SchedulerService(LocationService locationService)
    {
        _locationService = locationService;
    }

    // Location 인스턴스 조회
    public Location<ITransportable>? GetLocation(string id)
    {
        _locations.TryGetValue(id, out var location);
        return location;
    }

    // Location 관련 작업 예시
    public void LoadItem(string locationId, ITransportable item)
    {
        var location = GetLocation(locationId);
        if (location != null)
        {
            location.Load(item);
            // 필요 시 LocationService에 상태 동기화
        }
    }
}