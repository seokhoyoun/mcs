using Nexus.Core.Domain.Models.Areas; // AreaService를 사용하기 위해 추가
using Nexus.Core.Domain.Models.Locations;
using StackExchange.Redis;

namespace Nexus.Scheduler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AreaService _areaService; // AreaService 의존성 추가

        public Worker(ILogger<Worker> logger,  AreaService areaService)
        {
            _logger = logger;

            _areaService = areaService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _areaService.InitializeAreaService(); // AreaService 초기화 로직 추가

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}