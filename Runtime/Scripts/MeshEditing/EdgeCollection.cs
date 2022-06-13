using System;
using System.Collections.Generic;

namespace Unity.Geospatial.Streaming
{
    public struct Edge
    {
        public int Index1 { get; set; }
        public int Index2 { get; set; }
    }

    public class EdgeCollection
    {
        public List<Edge> Data { get; private set; }

        public EdgeCollection(List<Edge> edges)
        {
            Data = edges ?? throw new ArgumentNullException(nameof(edges));
        }
        public SortedSet<int> GetUniqueVertices()
        {
            SortedSet<int> uniqueVertices = new SortedSet<int>();

            foreach (Edge edge in Data)
            {
                uniqueVertices.Add(edge.Index1);
                uniqueVertices.Add(edge.Index2);
            }

            return uniqueVertices;
        }
    }
}
