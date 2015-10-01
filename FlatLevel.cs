using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project {
    public class FlatMap : Map {

        public override int Width { get; }

        public override int Height { get; }

        public FlatMap(int width, int height) {
            this.Width = width;
            this.Height = height;
        }

        public override UnitType this[int x, int y] {
            get {
                return UnitType.Floor;
            }
        }
    }
}
