using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Transports.Enums
{
    /// <summary>
    /// 운반 가능한 물품의 분류를 나타냅니다.
    /// </summary>
    public enum ECargoKind
    {
        Unknown = 0,
        Item = 1,
        Container = 2,
        Pallet = 3,
        Box = 4,
        Device = 5
    }
}
