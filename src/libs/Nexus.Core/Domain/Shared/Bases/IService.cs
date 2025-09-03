using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Shared.Bases
{
    public  interface IService
    {
        /// <summary>
        /// 데이터베이스 동기화 및 초기화 작업을 수행합니다.
        /// </summary>
        /// <param name="cancellationToken">작업 취소 토큰</param>
        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}
