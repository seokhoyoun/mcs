namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// 3축 회전(도 단위)을 나타내는 값 객체입니다. X(roll), Y(yaw), Z(pitch) 순서를 따릅니다.
    /// </summary>
    public class Rotation
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public Rotation(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}

