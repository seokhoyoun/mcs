using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Shared.Bases;
using System.Collections.Generic;

namespace Nexus.Core.Domain.Models.Transports.Interfaces
{
    /// <summary>
    /// 운반이 가능한 물품에만 필요한 인터페이스입니다.
    /// </summary>
    public interface ITransportable : IItem
    {
        ETransportType TransportType { get; }
    }
}