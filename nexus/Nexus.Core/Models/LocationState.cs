using Nexus.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Models
{
    /// <summary>
    /// Location 객체의 영구적인 상태를 나타내는 데이터 전송 객체(DTO)입니다.
    /// Redis에 저장될 Location 정보는 이 클래스를 통해 관리됩니다.
    /// </summary>
    public class LocationState
    {
        /// <summary>
        /// 위치의 고유 식별자입니다. (예: "ST01.CP01", "A01.SET01.MP01")
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// 위치의 이름입니다.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 해당 위치가 어떤 종류의 아이템인지 나타냅니다. (예: Cassette, Tray, Memory)
        /// </summary>
        public ELocationType LocationType { get; set; }

        /// <summary>
        /// 현재 이 포트에 적재된 아이템의 고유 식별자입니다. 아이템이 없으면 null입니다.
        /// </summary>
        public string? CurrentItemId { get; set; }

        // TODO: LocationStatus (Available, Occupied 등)를 나타내는 속성을 여기에 추가하고,
        // LocationService에서 이 속성을 업데이트하는 로직을 반영해야 합니다.
        // public LocationStatus Status { get; set; }
    }
}
