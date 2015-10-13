using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    public class MazeSolver
    {
        private Map map;
        private Graph<Vector2> graph;
        public Dictionary<Vector2, List<Vector2>> Paths {get; set;}

        public MazeSolver(Map map)
        {   
            this.map = map;
            graph = new Graph<Vector2>();
            
            // Add all map world unit locations as vertices in graph
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    graph.AddVertex(new Vector2(x, y));
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

                    Vector2 current = new Vector2(x, y);
                    Vector2 neighbourUp = new Vector2(x, y + 1);
                    Vector2 neighbourDown = new Vector2(x, y - 1);
                    //Vector2 neighbourLeft = new Vector2(x - 1, y);
                    Vector2 neighbourRight = new Vector2(x + 1, y);

                    //Vector2 neighbourUpLeft = new Vector2(x - 1, y + 1);
                    Vector2 neighbourUpRight = new Vector2(x + 1, y + 1);
                    //Vector2 neighbourDownLeft = new Vector2(x - 1, y - 1);
                    Vector2 neighbourDownRight = new Vector2(x + 1, y - 1);

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

        public Dictionary<Vector2, List<Vector2>> SolveMaze(Vector2 end)
        {
            Paths =  graph.Djkistra(end);
            return Paths;
        }

        // Add edge between current unit and neighbour unit if the neighbour
        // unit is within bound and is not a wall
        // Returns 0 if edge is added or exists. Returns 1 if edge is not possible
        private int AddEdge(Vector2 current, Vector2 neighbour)
        {
            if (!OutofBound(neighbour))
            {
                if (GetMapUnit(neighbour) != Map.UnitType.Wall)
                {
                    graph.AddEdge(current, neighbour, 1);
                    return 0;
                }
            }
            return 1;
        }

        private Map.UnitType GetMapUnit(Vector2 position) 
        {
            return map[(int)position.X, (int)position.Y];
        }

        private bool OutofBound(Vector2 location)
        {
            return (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height);
        }
    }
}
