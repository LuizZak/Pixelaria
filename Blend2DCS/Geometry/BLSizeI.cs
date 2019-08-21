using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend2DCS.Geometry
{
    public struct BLSizeI: IEquatable<BLSizeI>
    {
        public int Width;
        public int Height;

        public bool Equals(BLSizeI other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is BLSizeI other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Width * 397) ^ Height;
            }
        }

        public static bool operator ==(BLSizeI left, BLSizeI right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BLSizeI left, BLSizeI right)
        {
            return !left.Equals(right);
        }
    }
}
