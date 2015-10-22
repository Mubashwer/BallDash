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

        public abstract int Width { get; }
        public abstract int Height { get; }

        /// <summary>
        /// The width (X scale) of a map unit, in world coordinates
        /// </summary>
        public abstract float MapUnitWidth { get; }

        /// <summary>
        /// The height (Y scale) of a map unit, in world coordinates
        /// </summary>
        public abstract float MapUnitHeight { get; }

        /// <summary>
        /// The depth (Z scale) of a map unit, in world coordinates
        /// </summary>
        public abstract float MapUnitDepth { get; }


        public abstract Point StartPosition { get; }
        public abstract Point EndPosition { get; }

        public Vector2 GetMapUnitCoordinates(Vector2 worldCoordinates) {
            return new Vector2(worldCoordinates.X/ MapUnitWidth, worldCoordinates.Y / MapUnitHeight);
        }

        public Vector2 GetWorldCoordinates(Vector2 mapCoordinates) {
            return new Vector2(MapUnitWidth * mapCoordinates.X, MapUnitHeight * mapCoordinates.Y);
        }
    }

    public static class PointExtensions {
        public static Vector2 ToVector2(this Point point) {
            return new Vector2(point.X, point.Y);
        }
    }
}
