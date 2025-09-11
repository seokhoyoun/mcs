using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Robots.Interfaces;
using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Models.Robots.Services
{
    public class RobotService : BaseDataService<Robot, string>, IRobotService
    {
        private readonly IRobotRepository _robotRepository;
        private readonly List<Robot> _robots;

        public IReadOnlyList<Robot> Robots => _robots.AsReadOnly();

        public RobotService(ILogger<RobotService> logger, IRobotRepository robotRepository) : base(logger, robotRepository)
        {
            _robotRepository = robotRepository;
            _robots = new List<Robot>();
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Robot> robots = await _robotRepository.GetAllAsync(cancellationToken);

            if (robots == null || robots.Count == 0)
            {
                _logger.LogWarning("초기화된 Robot 데이터가 없습니다.");
                return;
            }

            _robots.Clear();
            _robots.AddRange(robots);
        }

        public async Task<bool> UpdatePositionAsync(string robotId, Position position, CancellationToken cancellationToken = default)
        {
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

