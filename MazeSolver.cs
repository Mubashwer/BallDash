using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX;
using SharpDX.Toolkit;

namespace Project {
    public class MazeSolver {
        private Map map;
        private Graph<Point> graph;
        MazeGame game;
        public Dictionary<Point, List<Point>> Paths { get; set; }
        public bool Enabled { get; set; }
        private Point? playerPosition = null;

        public MazeSolver(MazeGame game, Map map) {
            this.map = map;
            this.game = game;
            graph = new Graph<Point>();
            Enabled = false;

            // Add all map world unit locations as vertices in graph
            for (int x = 0; x < map.Width; x++) {
                for (int y = 0; y < map.Height; y++) {
                    graph.AddVertex(new Point(x, y));
                }
            }

            for (int x = 0; x < map.Width; x++) {
                for (int y = 0; y < map.Height; y++) {
                    var unitType = map[x, y];
                    if (unitType != Map.UnitType.Floor && unitType != Map.UnitType.PlayerEnd && unitType != Map.UnitType.PlayerStart) {
                        continue;
                    }

                    Point current = new Point(x, y);
                    Point neighbourUp = new Point(x, y + 1);
                    Point neighbourDown = new Point(x, y - 1);
                    //Point neighbourLeft = new Point(x - 1, y);
                    Point neighbourRight = new Point(x + 1, y);

                    //Point neighbourUpLeft = new Point(x - 1, y + 1);
                    Point neighbourUpRight = new Point(x + 1, y + 1);
                    //Point neighbourDownLeft = new Point(x - 1, y - 1);
                    Point neighbourDownRight = new Point(x + 1, y - 1);

                    // Variables for number of walls in respective direction
                    var UpRightWallCount = 0;
                    var DownRightWallCount = 0;
                    var RightWallCount = 0;

                    // Add possible edges (Up, Down and Right)
                    UpRightWallCount += AddEdge(current, neighbourUp);
                    DownRightWallCount += AddEdge(current, neighbourDown);
                    RightWallCount += AddEdge(current, neighbourRight);
                    UpRightWallCount += RightWallCount;
                    DownRightWallCount += RightWallCount;

                    //AddEdge(current, neighbourLeft);

                    // Do not add diagonal edge if there are horizontal and vertical walls blocking it
                    //if (UpRightWallCount < 2) AddEdge(current, neighbourUpRight);
                    //if (DownRightWallCount < 2) AddEdge(current, neighbourDownRight);
                }
            }


        }

        public void SolveMaze() {
            graph.Djkistra(map.EndPosition);
        }

        // Add edge between current unit and neighbour unit if the neighbour
        // unit is within bound and is not a wall
        // Returns 0 if edge is added or exists. Returns 1 if edge is not possible
        private int AddEdge(Point current, Point neighbour) {
            if (!OutofBound(neighbour)) {
                var mapUnit = GetMapUnit(neighbour);
                if (mapUnit == Map.UnitType.Floor || mapUnit == Map.UnitType.PlayerEnd || mapUnit == Map.UnitType.PlayerStart) {
                    graph.AddEdge(current, neighbour, 1);
                    return 0;
                }
            }
            return 1;
        }

        private Map.UnitType GetMapUnit(Point position) {
            return map[position.X, position.Y];
        }

        private bool OutofBound(Point location) {
            return (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height);
        }

        // Disables hint
        private void DisableHint() {
            foreach (var gameObject in game.GameObjects) {
                if (gameObject.IsHintObject) {
                    gameObject.IsHintObject = false;
                }
            }
            Enabled = false;
        }

        // Identity hint floor objects from djkistra's previous vertex to vertex path dictionary
        public void Hint() {
            if (!Enabled) return;
            var newPlayerPosition = game.Player.GetPlayerMapPoint();
            if (playerPosition != null && newPlayerPosition.Equals(playerPosition)) return;
            playerPosition = newPlayerPosition;
            //Debug.WriteLine("PLAYER MAP POSITION: " + playerPosition);
            DisableHint(); // reset hint objects 
            Enabled = true;
            Dictionary<Point, Point> previous = graph.previous;
            var current = (Point)playerPosition;
            try {

                if (!previous.ContainsKey(current)) {
                    Debug.WriteLine("NOT FOUND: " + map[current.X, current.Y]);
                    return;
                }

                game.Tiles[(Point)playerPosition].IsHintObject = true;
                while (previous.ContainsKey(previous[current])) {
                    if (current.Equals(map.EndPosition)) break;
                    var prev = previous[current];
                    try {
                        game.Tiles[prev].IsHintObject = true;
                    }
                    catch (Exception e) {
                        Debug.WriteLine(e);
                    }
                    current = prev;

                }
            }
            catch (Exception e) {
                Debug.WriteLine(e);
            }
        }
    }
}
