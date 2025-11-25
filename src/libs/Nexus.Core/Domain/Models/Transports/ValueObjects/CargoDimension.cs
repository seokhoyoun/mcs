using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Transports.ValueObjects
{
    /// <summary>
    /// 가로/세로/높이 규격을 나타내는 값 객체입니다.
    /// </summary>
    public sealed class CargoDimension
    {
        public CargoDimension(decimal width, decimal height, decimal depth)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
            }

            if (depth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be greater than zero.");
            }

            Width = width;
            Height = height;
            Depth = depth;
        }

        public decimal Width { get; }
        public decimal Height { get; }
        public decimal Depth { get; }

        public bool FitsIn(CargoDimension limit)
        {
            if (limit == null)
            {
                throw new ArgumentNullException(nameof(limit));
            }

            if (Width > limit.Width)
            {
                return false;
            }

            if (Height > limit.Height)
            {
                return false;
            }

            if (Depth > limit.Depth)
            {
                return false;
            }

            return true;
        }
    }
}
