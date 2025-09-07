using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;

namespace Nexus.Core.Domain.Models.Locations
{
    /// <summary>
    /// �κ�(AMR)�� ī��Ʈ/Ʈ���� ��Ʈ�� ��Ÿ���� Location Ŭ����
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