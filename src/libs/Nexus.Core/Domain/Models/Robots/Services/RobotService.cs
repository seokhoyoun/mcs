using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Robots.Services
{
    public class RobotService : BaseDataService<Robot, string>, IRobotService
    {
        private readonly IRobotRepository _robotRepository;
        private readonly List<Robot> _robots;
        private bool _initialized = false;
        private readonly object _initLock = new object();
        private Task? _initTask;

        public IReadOnlyList<Robot> Robots => _robots.AsReadOnly();

        public RobotService(ILogger<RobotService> logger, IRobotRepository robotRepository) : base(logger, robotRepository)
        {
            _robotRepository = robotRepository;
            _robots = new List<Robot>();
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
        {
            if (_initialized)
            {
                return;
            }
            Task? startTask = null;
            lock (_initLock)
            {
                if (_initialized)
                {
                    return;
                }
                if (_initTask == null)
                {
                    _initTask = InitializeCoreAsync(cancellationToken);
                }
                startTask = _initTask;
            }
            await startTask;
        }

        private async Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                IReadOnlyList<Robot> robots = await _robotRepository.GetAllAsync(cancellationToken);

                _robots.Clear();
                if (robots != null && robots.Count > 0)
                {
                    _robots.AddRange(robots);
                }
                else
                {
                    _logger.LogWarning("초기화된 Robot 데이터가 없습니다.");
                }
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RobotService 초기화 중 오류 발생");
                throw;
            }
        }

        public async Task<bool> UpdatePositionAsync(string robotId, Position position, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            bool updated = await _robotRepository.UpdatePositionAsync(robotId, position, cancellationToken);
            if (updated)
            {
                Robot? local = _robots.FirstOrDefault(r => r.Id == robotId);
                if (local != null)
                {
                    local.Position = position;
                }
            }
            return updated;
        }

        public async Task<Position?> GetPositionAsync(string robotId, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            Robot? local = _robots.FirstOrDefault(r => r.Id == robotId);
            if (local != null)
            {
                return local.Position;
            }
            Position? fromRepo = await _robotRepository.GetPositionAsync(robotId, cancellationToken);
            return fromRepo;
        }

        public Robot? GetRobotByIdCached(string id)
        {
            return _robots.FirstOrDefault(r => r.Id == id);
        }
    }
}

