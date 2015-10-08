using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    public class Graph<T> where T: IEquatable<T>
    {
        private Dictionary<T, Dictionary<T, int>> adjacencyList = new Dictionary<T, Dictionary<T, int>>();
        public int Vertices { get; private set; }

        public int Edges { get; private set; }

        public bool AddVertex(T vertexKey)
        {
            if (!adjacencyList.ContainsKey(vertexKey))
            {
                adjacencyList[vertexKey] = new Dictionary<T, int>();
                Vertices++;
                return true;
            }
            return false;
        }

        public bool AddEdge(T vertexKeyA, T vertexKeyB, int cost)
        {
            if (!adjacencyList.ContainsKey(vertexKeyA) || !adjacencyList.ContainsKey(vertexKeyB))
                return false;
            (adjacencyList[vertexKeyA])[vertexKeyB] = cost;
            (adjacencyList[vertexKeyB])[vertexKeyA] = cost;
            Edges++;
            return true;
        }

        public List<T> ShortestPath(T start, T finish)
        {
            var previous = new Dictionary<T, T>();
            var distances = new Dictionary<T, int>();
            var nodes = new List<T>();

            List<T> path = null;

            foreach (var vertex in adjacencyList)
            {
                if (vertex.Key.Equals(start))
                {
                    distances[vertex.Key] = 0;
                }
                else
                {
                    distances[vertex.Key] = int.MaxValue;
                }

                nodes.Add(vertex.Key);
            }

            while (nodes.Count != 0)
            {
                nodes.Sort((x, y) => distances[x] - distances[y]);

                var smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest.Equals(finish))
                {
                    path = new List<T>();
                    while (previous.ContainsKey(smallest))
                    {
                        path.Add(smallest);
                        smallest = previous[smallest];
                    }

                    break;
                }

                if (distances[smallest] == int.MaxValue)
                {
                    break;
                }

                foreach (var neighbor in adjacencyList[smallest])
                {
                    var alt = distances[smallest] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = smallest;
                    }
                }
            }

            return path;
        }
    }
}
