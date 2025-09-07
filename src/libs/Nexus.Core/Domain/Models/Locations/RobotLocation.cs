using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;

namespace Nexus.Core.Domain.Models.Locations
{
    /// <summary>
    /// 로봇(AMR)의 카세트/트레이 포트를 나타내는 Location 클래스
    /// </summary>
    public class RobotLocation : Location
    {
        public override ITransportable? CurrentItem
        {
            get => _currentItem;
            set => _currentItem = value;
        }

        private ITransportable? _currentItem;

        public RobotLocation(string id, string name, ELocationType locationType) : base(id, name, locationType)
        {
        }
    }
}