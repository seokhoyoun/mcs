using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Models.Transports.ValueObjects;
using Nexus.Core.Domain.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Transports.Interfaces
{
    /// <summary>
    /// 운반 가능한 모든 물건(운반체 포함)을 표현합니다.
    /// </summary>
    public interface ICargo : IItem
    {
        /// <summary>
        /// 물건의 분류입니다.
        /// </summary>
        ECargoKind Kind { get; }

        /// <summary>
        /// 외형 규격입니다.
        /// </summary>
        CargoDimension Size { get; }

        /// <summary>
        /// 무게(kg 기준 등)입니다.
        /// </summary>
        decimal Weight { get; }
    }
}
