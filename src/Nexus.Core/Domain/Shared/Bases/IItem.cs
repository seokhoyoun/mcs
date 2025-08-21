namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// 모든 물품(운반 단위, 내용물)의 기본 인터페이스입니다.
    /// </summary>
    public interface IItem : IEntity
    {
        /// <summary>
        /// 운반 단위에 포함된 물품 목록
        /// </summary>
        IReadOnlyList<IItem>? Items { get; }
    }
}