using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project {
    public abstract class Map {
        public enum UnitType {
            None = 0,
            Floor = 1,
            Wall = 2,
            SmallHole1 = 3,
            SmallHole2 = 4,
            SmallHole3 = 5,
            SmallHole4 = 6
        }

        public abstract UnitType this[int x, int y] { get; }

        public abstract int Width { get; }
        public abstract int Height { get; }
    }
}
