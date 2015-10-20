using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    public class MazeSolver
    {
        private Map map;
        private Graph<Point> graph;
        LabGame game;
        public Dictionary<Point, List<Point>> Paths {get; set;}
        private bool Enabled { get; set; }
        private Point playerPosition;

        public MazeSolver(LabGame game, Map map)
        {   
            this.map = map;
            this.game = game;
            graph = new Graph<Point>();
            Enabled = false;
            var playerPositionVector = map.GetMapUnitCoordinates(new Vector2(game.player.position.X, game.player.position.Y));
            playerPosition = new Point((int)playerPositionVector.X, (int)playerPositionVector.Y);
            
            // Add all map world unit locations as vertices in graph
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    graph.AddVertex(new Point(x, y));
                }
            }

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    if (map[x, y] == Map.UnitType.Wall)
                    {
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

                    // Do not add diagonal edge if there are horizontal and vertical walls blocking it
                    if(UpRightWallCount < 2) AddEdge(current, neighbourUpRight);
                    if(DownRightWallCount < 2) AddEdge(current, neighbourDownRight);
                }
            }


        }

        public void SolveMaze()
        {
            graph.Djkistra(map.EndPosition);
        }

        // Add edge between current unit and neighbour unit if the neighbour
        // unit is within bound and is not a wall
        // Returns 0 if edge is added or exists. Returns 1 if edge is not possible
        private int AddEdge(Point current, Point neighbour)
        {
            if (!OutofBound(neighbour))
            {
                var mapUnit = GetMapUnit(neighbour);
                if (mapUnit == Map.UnitType.Floor || mapUnit == Map.UnitType.PlayerEnd || mapUnit == Map.UnitType.PlayerStart)
                {
                    graph.AddEdge(current, neighbour, 1);
                    return 0;
                }
            }
            return 1;
        }

        private Map.UnitType GetMapUnit(Point position) 
        {
            return map[(int)position.X, (int)position.Y];
        }

        private bool OutofBound(Point location)
        {
            return (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height);
        }

        private void ResetHint() {
            foreach (var gameObject in game.gameObjects)
            {
                if (gameObject.IsHintObject)
                {
                    gameObject.IsHintObject = false;
                }
            }
        }

        public void Hint()
        {          
            var newPlayerPositionVector = map.GetMapUnitCoordinates(new Vector2(game.player.position.X, game.player.position.Y));
            var newPlayerPosition = new Point((int)newPlayerPositionVector.X, (int)newPlayerPositionVector.Y);
            if (newPlayerPosition.Equals(playerPosition)) return;
            playerPosition = newPlayerPosition;
            Debug.WriteLine("PLAYER MAP POSITION: " + playerPosition);
            ResetHint();
            Dictionary<Point, Point> previous = graph.previous;
            var current = playerPosition;
            //try
            //{
                

                current = playerPosition;

                if (!previous.ContainsKey(current)) {
                    Debug.WriteLine("NOT FOUND: " + map[current.X, current.Y]);
                    return;
                }
                game.tiles[playerPosition].IsHintObject = true;
                while (previous.ContainsKey(previous[current]))
                {
                    if (current.Equals(map.EndPosition)) break;
                    var prev = previous[current];
                    game.tiles[prev].IsHintObject = true;
                    current = prev;

                }
           // }
            //catch {
             //   Debug.WriteLine("current: " + current);
               // Debug.WriteLine("previous" + previous);
            //}
        }
    }
}
