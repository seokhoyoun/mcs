using Nexus.Core.Domain.Shared.Events;

namespace Nexus.Core.Domain.Shared.Interfaces
{
    /// <summary>
    /// 고유 식별자를 가지는 객체의 공통 인터페이스입니다.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// 고유 식별자
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 이름
        /// </summary>
        string Name { get; }
     
    }
}