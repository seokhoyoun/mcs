using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Shared.Application.DTO
{
    /// <summary>
    /// Location 객체의 상태를 나타내는 데이터 전송 객체(DTO)입니다.
    /// </summary>
    public class LocationState
    {
        public string Id { get; private set; }

        /// <summary>
        /// 현재 이 포트에 적재된 아이템의 고유 식별자입니다. 아이템이 없으면 null입니다.
        /// </summary>
        public string? CurrentItemId { get; private set; }

        /// <summary>
        /// 현재 위치의 상태를 나타냅니다. 
        /// </summary>
        public int Status { get; private set; }

        public LocationState(string id, string? currentItemId, int status)
        {
            Id = id;
            CurrentItemId = currentItemId;
            Status = status;
        }
    }
}
