using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Transports.Interfaces
{
    public interface ITransportsRepository
    {
        /// <summary>
        /// 현재 존재하는 모든 카세트(Cassette) 목록을 반환합니다.
        /// </summary>
        IEnumerable<Cassette> GetAllCassettes();

        /// <summary>
        /// 현재 존재하는 모든 트레이(Tray) 목록을 반환합니다.
        /// </summary>
        IEnumerable<Tray> GetAllTrays();

        /// <summary>
        /// 현재 존재하는 모든 메모리(Memory) 목록을 반환합니다.
        /// </summary>
        IEnumerable<Memory> GetAllMemories();
    }
}
