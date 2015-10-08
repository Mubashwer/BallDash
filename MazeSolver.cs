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
                    Vector2 neighbourLeft = new Vector2(x - 1, y);
                    Vector2 neighbourRight = new Vector2(x + 1, y);

                    AddEdge(current, neighbourUp);
                    AddEdge(current, neighbourDown);
                    AddEdge(current, neighbourLeft);
                    AddEdge(current, neighbourRight);

                }
            }


        }

        public List<Vector2> Path(Vector2 start, Vector2 end)
        {
            return graph.ShortestPath(start, end);
        }

        // Add edge between current unit and neighbour unit if the neighbour
        // unit is within bound and is not a wall
        private void AddEdge(Vector2 current, Vector2 neighbour)
        {
            if (!OutofBound(neighbour))
            {
                if (map[(int)neighbour.X, (int)neighbour.Y] != Map.UnitType.Wall)
                {
                    graph.AddEdge(current, neighbour, 1);
                }
            }
        }

        private bool OutofBound(Vector2 location)
        {
            return (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height);
        }
    }
}
