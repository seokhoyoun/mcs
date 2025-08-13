using Nexus.Core.Domain.Models.Areas; // AreaService�� ����ϱ� ���� �߰�
using Nexus.Core.Domain.Models.Locations;
using StackExchange.Redis;

namespace Nexus.Scheduler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AreaService _areaService; // AreaService ������ �߰�

        public Worker(ILogger<Worker> logger,  AreaService areaService)
        {
            _logger = logger;

            _areaService = areaService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _areaService.InitializeAreaService(); // AreaService �ʱ�ȭ ���� �߰�

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}