namespace Nexus.Core.Domain.Models.Locations.Interfaces
{
    /// <summary>
    /// 아이템을 저장할 수 있는 위치를 나타내는 인터페이스입니다.
    /// </summary>
    public interface IItemStorage
    {
        /// <summary>
        /// 현재 저장된 아이템의 식별자입니다.
        /// 비어있는 경우 빈 문자열을 반환합니다.
        /// </summary>
        string CurrentItemId { get; set; }
        
        /// <summary>
        /// 현재 위치에 아이템이 저장되어 있는지 여부를 반환합니다.
        /// </summary>
        bool HasItem => !string.IsNullOrEmpty(CurrentItemId);

    }
}
