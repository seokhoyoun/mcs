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
    public class LocationInfo
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

        /// <summary>
        /// 현재 위치의 상태를 나타냅니다. 
        /// </summary>
        public ELocationStatus Status { get; set; }
    }
}
