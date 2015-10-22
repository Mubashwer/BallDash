using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Project {
    public abstract class Map {
        [Flags]
        public enum UnitType {
            None = 0,
            Floor = 1,
            Wall = 2,
            PlayerStart = 4,
            PlayerEnd = 8,
            Hole = 16,
            Rainbow = 32
        }

        public abstract UnitType this[int x, int y] { get; }

        public abstract int Width { get; set; }
        public abstract int Height { get; set; }

        public abstract Point StartPosition { get; set; }
        public abstract Point EndPosition { get; set; }

        public Vector2 GetMapUnitCoordinates(Vector2 worldCoordinates) {
            return new Vector2(((worldCoordinates.X - (WorldUnitWidth / 2)) / WorldUnitWidth), ((worldCoordinates.Y - (WorldUnitHeight / 2)) / WorldUnitHeight));
        }

        public static readonly float WorldUnitWidth = 3f;
        public static readonly float WorldUnitHeight = 3f;
    }

    public static class PointExtensions {
        public static Vector2 ToVector2(this Point point) {
            return new Vector2(point.X, point.Y);
        }
    }
}
