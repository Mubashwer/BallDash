using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Project {
    public class TextMap : Map {
        public const char WallCharacter = 'x';
        public const char HoleCharacter = 'o';
        public const char BlankCharacter = ' ';
        public const char FloorCharacter = '.';
        public const char PlayerStartCharacter = 's';
        public const char PlayerEndCharacter = 'e';
        public const char RainbowCharacter = 'r';

        private List<List<UnitType>> mapDefinition;

        private Point startPosition;
        private Point endPosition;
        public override Point StartPosition { get { return startPosition; } }
        public override Point EndPosition { get { return endPosition; } }

        private int height;
        private int width;
        public override int Height { get { return height; } }
        public override int Width { get { return width; } }

        private float mapUnitWidth;
        private float mapUnitHeight;
        private float mapUnitDepth;
        public override float MapUnitWidth { get { return mapUnitWidth; } }
        public override float MapUnitHeight { get { return mapUnitHeight; } }
        public override float MapUnitDepth { get { return mapUnitDepth; } }

        public TextMap(IList<string> mapLines, float mapUnitWidth, float mapUnitHeight, float mapUnitDepth) : base() {
            this.mapUnitWidth = mapUnitWidth;
            this.mapUnitHeight = mapUnitHeight;
            this.mapUnitDepth = mapUnitDepth;

            GenerateMap(mapLines);
        }

        private void GenerateMap(IList<string> mapLines) {
            List<List<UnitType>> map = new List<List<UnitType>>();

            int maxWidth = -1;
            // (0,0) on the map is the bottom left, so we need to iterate j backwards
            for (int j = mapLines.Count - 1; j >= 0; j--) {
                List<UnitType> currentRow = new List<UnitType>();
                for (int i = 0; i < mapLines[j].Length; i++) {
                    char currentChar = mapLines[j][i];

                    // add different unit type depending on character
                    switch (currentChar) {
                        case BlankCharacter:
                            currentRow.Add(UnitType.None);
                            break;
                        case FloorCharacter:
                            currentRow.Add(UnitType.Floor);
                            break;
                        case WallCharacter:
                            currentRow.Add(UnitType.Wall);
                            break;
                        case HoleCharacter:
                            currentRow.Add(UnitType.Hole);
                            break;
                        case PlayerStartCharacter:
                            currentRow.Add(UnitType.PlayerStart | UnitType.Floor);
                            break;
                        case PlayerEndCharacter:
                            currentRow.Add(UnitType.PlayerEnd | UnitType.Floor);
                            break;
                        case RainbowCharacter:
                            currentRow.Add(UnitType.Rainbow | UnitType.Floor);
                            break;
                    }
                }

                // update maximum width
                if (maxWidth < mapLines[j].Length) {
                    maxWidth = mapLines[j].Length;
                }

                map.Add(currentRow);
            }

            this.width = maxWidth;
            this.height = map.Count;

            mapDefinition = map;

            UpdateStartEndPositions();
        }

        private void UpdateStartEndPositions() {
            for (int i = 0; i < this.Height; i++) {
                for (int j = 0; j < this.Width; j++) {
                    if (this[j, i].HasFlag(UnitType.PlayerStart)) {
                        startPosition = new Point(j, i);
                    }

                    if (this[j, i].HasFlag(UnitType.PlayerEnd)) {
                        endPosition = new Point(j, i);
                    }
                }
            }
        }

        public override UnitType this[int x, int y] {
            get {
                // if the x and y is out of range, just return UnitType.None
                if ((mapDefinition == null)
                    || (x < 0)
                    || (y < 0)
                    || (y >= mapDefinition.Count)
                    || (x >= mapDefinition[y].Count)) {

                    return UnitType.None;
                }

                // return the unit type at the coordinate
                return mapDefinition[y][x];
            }
        }

        public override string ToString() {
            StringBuilder output = new StringBuilder();
            for (int j = 0; j < this.Height; j++) {
                output.Append(string.Join(",", mapDefinition[j]));
                output.AppendLine();
            }

            return output.ToString();
        }
    }
}
