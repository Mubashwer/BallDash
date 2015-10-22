using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project {
    public class LevelInfo {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Map Map { get; set; }

        private const string ParserAreaStartString = "[";
        private const string ParserAreaEndString = "]";
        private const string ParserEqualsString = "=";

        public static LevelInfo CreateFromMapDefinition(IEnumerable<string> mapDefinitionLines) {
            LevelInfo levelInfo = new LevelInfo();

            // keep track of what part of the document we're in
            ParserArea currentArea = ParserArea.None;

            // for adding map lines to
            List<string> mapLines = new List<string>();
            float mapUnitWidth = 0;
            float mapUnitHeight = 0;
            float mapUnitDepth = 0;


            // for adding light lines to
            List<string> lightLines = new List<string>();

            foreach (string thisLine in mapDefinitionLines) {
                // look for an area tag
                int areaStartIndex = thisLine.IndexOf(ParserAreaStartString) + ParserAreaStartString.Length;
                // check if we found the start of an area tag
                if (areaStartIndex > 0) {
                    // check if we found the end of an area tag
                    int areaEndIndex = thisLine.IndexOf(ParserAreaEndString);
                    if (areaEndIndex > areaStartIndex) {
                        // get the name of the area from start and end index
                        string areaName = thisLine.Substring(areaStartIndex, areaEndIndex - areaStartIndex);

                        // set the current area based on area name
                        if (areaName.Equals("Settings", StringComparison.OrdinalIgnoreCase)) {
                            currentArea = ParserArea.Settings;
                        }
                        else if (areaName.Equals("Map", StringComparison.OrdinalIgnoreCase)) {
                            currentArea = ParserArea.Map;
                        }
                        else if (areaName.Equals("End", StringComparison.OrdinalIgnoreCase)) {
                            currentArea = ParserArea.None;
                        }
                        else {
                            Debug.WriteLine("Unknown area tag: " + areaName);
                            currentArea = ParserArea.None;
                        }

                        // we're done parsing this line
                        continue;
                    }
                }

                // if we're in an area, handle the area appropriatly
                // ignore everything that isn't in an area
                if (currentArea == ParserArea.None) continue;

                if (currentArea == ParserArea.Settings) {
                    // ignore blank lines
                    if(thisLine.Trim().Length == 0) continue;

                    // we're in the settings area, start parsing key value pairs for variables
                    KeyValuePair<string, string> currentVariable;
                    if (!TryGetKeyValueFromEqualSeparatedString(thisLine, out currentVariable)) {
                        Debug.WriteLine("Could not parse line in map file under Settings area: " + currentVariable.Key);
                        continue;
                    }

                    // handle the variable that we have
                    if (currentVariable.Key.Equals("Index", StringComparison.OrdinalIgnoreCase)) {
                        int index;
                        if (!int.TryParse(currentVariable.Value, out index)) {
                            Debug.WriteLine("Could not parse variable in map file under Settings area: "
                                + currentVariable.Key + ":" + currentVariable.Value);
                            continue;
                        }

                        levelInfo.Index = index;
                    }
                    else if (currentVariable.Key.Equals("Name", StringComparison.OrdinalIgnoreCase)) {
                        levelInfo.Name = currentVariable.Value;
                    }
                    else if (currentVariable.Key.Equals("Description", StringComparison.OrdinalIgnoreCase)) {
                        levelInfo.Description = currentVariable.Value;
                    }
                    else if (currentVariable.Key.Equals("MapUnitWidth", StringComparison.OrdinalIgnoreCase)) {
                        if (!float.TryParse(currentVariable.Value, out mapUnitWidth)) {
                            Debug.WriteLine("Could not parse variable in map file under Settings area: "
                                + currentVariable.Key + ":" + currentVariable.Value);
                            continue;
                        }
                    }
                    else if (currentVariable.Key.Equals("MapUnitHeight", StringComparison.OrdinalIgnoreCase)) {
                        if (!float.TryParse(currentVariable.Value, out mapUnitHeight)) {
                            Debug.WriteLine("Could not parse variable in map file under Settings area: "
                                + currentVariable.Key + ":" + currentVariable.Value);
                            continue;
                        }
                    }
                    else if (currentVariable.Key.Equals("MapUnitDepth", StringComparison.OrdinalIgnoreCase)) {
                        if (!float.TryParse(currentVariable.Value, out mapUnitDepth)) {
                            Debug.WriteLine("Could not parse variable in map file under Settings area: "
                                + currentVariable.Key + ":" + currentVariable.Value);
                            continue;
                        }
                    }
                    else {
                        Debug.WriteLine("Unknown variable in map file under Settings area: "
                                + currentVariable.Key + ":" + currentVariable.Value);
                    }
                }
                else if (currentArea == ParserArea.Map) {
                    mapLines.Add(thisLine);
                }
            }

            // everything is parsed, now generate the map
            levelInfo.Map = new TextMap(mapLines, mapUnitWidth, mapUnitHeight, mapUnitDepth);

            return levelInfo;
        }

        private static bool TryGetKeyValueFromEqualSeparatedString(string input, out KeyValuePair<string, string> keyValuePair) {
            int indexOfEquals = input.IndexOf(ParserEqualsString);
            if (indexOfEquals < 0) {
                return false;
            }

            string key = input.Substring(0, indexOfEquals).Trim();

            if (key.Length == 0) {
                return false;
            }

            string value = input.Substring(indexOfEquals + ParserEqualsString.Length);

            keyValuePair = new KeyValuePair<string, string>(key, value);

            return true;
        }

        private enum ParserArea {
            None,
            Settings,
            Map
        }
    }
}
