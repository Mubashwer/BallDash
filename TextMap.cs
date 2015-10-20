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
        public const char PlayerStartCharacter = 's';
        public const char PlayerEndCharacter = 'e';

        private List<List<UnitType>> mapDefinition;

        private int width;
        private int height;

        public TextMap(string mapPath) {
            GenerateMap(mapPath);
        }

        private void GenerateMap(string mapPath) {
            IList<string> readLines = null;

            try {
                StorageFolder folder = Package.Current.InstalledLocation;
                StorageFile file = folder.GetFileAsync(mapPath).GetAwaiter().GetResult();
                readLines = FileIO.ReadLinesAsync(file).GetAwaiter().GetResult();
            }
            catch (Exception e) {
                Debug.WriteLine("Could not open map {0}, Error: {1}", mapPath, e.ToString());
                return;
            }

            List<List<UnitType>> map = new List<List<UnitType>>();

            int maxWidth = -1;
            // (0,0) on the map is the bottom left, so we need to iterate j backwards
            for (int j = readLines.Count -1; j >= 0 ; j--) {
                List<UnitType> currentRow = new List<UnitType>();
                for (int i = 0; i < readLines[j].Length; i++) {
                    char currentChar = readLines[j][i];

                    // add different unit type depending on character
                    switch (currentChar) {
                        case BlankCharacter:
                            currentRow.Add(UnitType.None);
                            break;
                        case WallCharacter:
                            currentRow.Add(UnitType.Wall);
                            break;
                        case HoleCharacter:
                            currentRow.Add(UnitType.SmallHole1);
                            break;
                        case PlayerStartCharacter:
                            currentRow.Add(UnitType.PlayerStart);
                            break;
                        case PlayerEndCharacter:
                            currentRow.Add(UnitType.PlayerEnd);
                            break;
                    }
                }

                // update maximum width
                if (maxWidth < readLines[j].Length) {
                    maxWidth = readLines[j].Length;
                }

                map.Add(currentRow);
            }

            this.width = maxWidth;
            this.height = map.Count;

            mapDefinition = map;
        }

        public override UnitType this[int x, int y] {
            get {
                // if the x and y is out of range, just return UnitType.None
                if ((mapDefinition == null)
                    || (y >= mapDefinition.Count)
                    || (x >= mapDefinition[y].Count)) {

                    return UnitType.None;
                }

                // return the unit type at the coordinate
                return mapDefinition[y][x];
            }
        }

        public override int Height {
            get {
                return this.height;
            }
            set
            {
                this.width = value;
            }
        }

        public override int Width {
            get {
                return this.width;
            }
            set
            {
                this.width = value;
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
