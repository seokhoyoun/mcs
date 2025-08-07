namespace Nexus.Core.Domain.Shared
{
    /// <summary>
    /// 창고 내 위치의 현재 상태를 정의하는 열거형입니다.
    /// </summary>
    public enum ELocationStatus
    {
        /// <summary>
        /// 정의되지 않은 상태입니다.
        /// </summary>
        Undefined,

        /// <summary>
        /// 위치를 사용할 수 있으며, 아이템을 적재하거나 진입할 수 있습니다.
        /// </summary>
        Available,

        /// <summary>
        /// 위치가 현재 아이템으로 점유되어 있거나, 로봇이 진입하여 작업을 수행 중입니다.
        /// </summary>
        Occupied,

        /// <summary>
        /// 유지보수, 고장 등의 이유로 위치를 사용할 수 없는 상태입니다.
        /// </summary>
        OutOfService,

        /// <summary>
        /// 예약된 상태로, 곧 사용될 예정이지만 아직 점유되지는 않았습니다.
        /// </summary>
        Reserved
    }
}
