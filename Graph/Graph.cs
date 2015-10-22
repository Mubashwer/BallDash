using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    public class Graph<T> where T : IEquatable<T>
    {
        private Dictionary<T, Dictionary<T, int>> adjacencyList = new Dictionary<T, Dictionary<T, int>>();
        
        public Dictionary<T,T> previous = new Dictionary<T, T>();
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

        public void Djkistra(T source)
        {
            var distances = new Dictionary<T, int>();
            var vertices = new PriorityQueue<T, int>();

            foreach (var vertex in adjacencyList.Keys)
            {
                if (vertex.Equals(source))
                    distances[vertex] = 0;
                else
                    distances[vertex] = int.MaxValue;

                vertices[vertex] = distances[vertex];
            }

            while (vertices.Count != 0)
            {
                var minimum = vertices.Peek();
                vertices.Pop();

                if (distances[minimum] == int.MaxValue)
                    break;

                foreach (var neighbourEdge in adjacencyList[minimum])
                {
                    var alt = distances[minimum] + neighbourEdge.Value;
                    if (alt < distances[neighbourEdge.Key])
                    {
                        distances[neighbourEdge.Key] = alt;
                        vertices[neighbourEdge.Key] = distances[neighbourEdge.Key];
                        previous[neighbourEdge.Key] = minimum;
                    }
                }
            }
            //ConstructPaths(source);
        }

        //TESTING
        private void ConstructPaths(T source)
        {
            Dictionary<T, List<T>> paths = new Dictionary<T, List<T>>();
            
            foreach(var vertex in previous.Keys)
            {
                paths[vertex] = new List<T>();
                paths[vertex].Add(vertex);

                var current = vertex;
                while (previous.ContainsKey(previous[current]))
                {
                    if (current.Equals(source)) break;
                    var prev = previous[current];
                    paths[vertex].Add(prev);
                    current = prev;
                }
                if (previous[current].Equals(source)) paths[vertex].Add(source);
            }
            foreach (var path in paths.Values)
            {
                Debug.WriteLine("STARTPATH");
                foreach(var unit in path)
                Debug.WriteLine(unit);
                Debug.WriteLine("ENDPATH");
            }
        }

    }
}
