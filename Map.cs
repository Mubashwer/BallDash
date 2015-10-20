using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Project {
    public abstract class Map {
        public enum UnitType {
            None = 0,
            Floor = 1,
            Wall = 2,
            PlayerStart = 11,
            PlayerEnd = 12,
            SmallHole1 = 20,
            SmallHole2 = 21,
            SmallHole3 = 22,
            SmallHole4 = 23
        }

        public abstract UnitType this[int x, int y] { get; }

        public abstract int Width { get; set; }
        public abstract int Height { get; set; }

        public Vector2 GetMapCoordinates(Vector2 worldCoordinates) {
            return new Vector2(worldCoordinates.X / WorldUnitWidth, worldCoordinates.Y / WorldUnitHeight);
        }

        public static readonly float WorldUnitWidth = 3f;
        public static readonly float WorldUnitHeight = 3f;
    }
}
