using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Shared.Bases;
using System.Collections.Generic;

namespace Nexus.Core.Domain.Models.Transports.Interfaces
{
    /// <summary>
    /// ����� ������ ��ǰ���� �ʿ��� �������̽��Դϴ�.
    /// </summary>
    public interface ITransportable : IItem
    {
        public ETransportType TransportType { get; }
        public Location? CurrentLocation { get; set; }
    }
}