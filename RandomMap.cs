using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project {
    public class RandomMap : Map {
        private UnitType[,] level;

        public RandomMap(int width, int height) {
            this.Width = width;
            this.Height = height;
            GenerateMap();
        }

        private void GenerateMap() {
            throw new NotImplementedException();
        }

        public override UnitType this[int x, int y] {
            get {
                throw new NotImplementedException();
            }
        }

        public override int Height { get; set; }

        public override int Width { get; set; }
    }
}
