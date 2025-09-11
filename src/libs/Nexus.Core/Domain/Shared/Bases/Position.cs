namespace Nexus.Core.Domain.Shared.Bases
{
    /// <summary>
    /// 3차원 위치 좌표를 나타내는 값 객체입니다.
    /// 창고 내 설비와 포트의 물리적 위치를 표현합니다.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// X 좌표 (가로축)
        /// </summary>
        public uint X { get; }

        /// <summary>
        /// Y 좌표 (세로축)
        /// </summary>
        public uint Y { get; }

        /// <summary>
        /// Z 좌표 (높이축)
        /// </summary>
        public uint Z { get; }

        /// <summary>
        /// Position 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <param name="z">Z 좌표</param>
        public Position(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// 다른 Position과의 거리를 계산합니다.
        /// </summary>
        /// <param name="other">비교할 다른 Position</param>
        /// <returns>유클리드 거리</returns>
        public double CalculateDistanceTo(Position other)
        {
            double deltaX = (double)X - other.X;
            double deltaY = (double)Y - other.Y;
            double deltaZ = (double)Z - other.Z;
            
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        /// <summary>
        /// 현재 Position이 다른 Position과 같은지 확인합니다.
        /// </summary>
        /// <param name="obj">비교할 객체</param>
        /// <returns>같으면 true, 다르면 false</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not Position other)
            {
                return false;
            }

            return X == other.X && Y == other.Y && Z == other.Z;
        }

        /// <summary>
        /// Position의 해시 코드를 반환합니다.
        /// </summary>
        /// <returns>해시 코드</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        /// <summary>
        /// Position의 문자열 표현을 반환합니다.
        /// </summary>
        /// <returns>좌표 문자열</returns>
        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        /// <summary>
        /// 두 Position이 같은지 비교합니다.
        /// </summary>
        public static bool operator ==(Position? left, Position? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// 두 Position이 다른지 비교합니다.
        /// </summary>
        public static bool operator !=(Position? left, Position? right)
        {
            return !(left == right);
        }
    }
}
