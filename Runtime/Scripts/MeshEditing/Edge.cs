using System.Collections.Generic;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Define a polygon edge with two <see href="https://docs.unity3d.com/ScriptReference/Mesh-vertices.html">vertices</see> indices.
    /// </summary>
    public struct Edge
    { 
        /// <summary>
        /// Index of the <see href="https://docs.unity3d.com/ScriptReference/Mesh-vertices.html">vertices</see> representing
        /// the start of the edge.
        /// </summary>
        public int Index1 { get; set; }
        
        /// <summary>
        /// Index of the <see href="https://docs.unity3d.com/ScriptReference/Mesh-vertices.html">vertices</see> representing
        /// the end of the edge.
        /// </summary>
        public int Index2 { get; set; }
    }

    /// <summary>
    /// List of <see cref="Edge">Edges</see>.
    /// </summary>
    public static class EdgeCollection
    {
        /// <summary>
        /// Convert the List of <see cref="Edge">Edges</see> to a
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.sortedset-1">SortedSet</see>
        /// of <see href="https://docs.unity3d.com/ScriptReference/Mesh-vertices.html">vertices</see> indices where
        /// every two value is a new edge.
        /// </summary>
        /// <param name="edges">The list to convert.</param>
        /// <returns>The list result.</returns>
        public static SortedSet<int> GetUniqueVertices(this List<Edge> edges)
        {
            SortedSet<int> uniqueVertices = new SortedSet<int>();

            foreach (Edge edge in edges)
            {
                uniqueVertices.Add(edge.Index1);
                uniqueVertices.Add(edge.Index2);
            }

            return uniqueVertices;
        }
    }
}
