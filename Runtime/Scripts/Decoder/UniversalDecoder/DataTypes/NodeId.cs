using System;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{

    /// <summary>
    /// Indirection to a single item part of the <see cref="BoundingVolumeHierarchy{T}"/>
    /// </summary>
    public readonly struct NodeId : IEquatable<NodeId>
    {
        /// <summary>
        /// Index of the <see cref="NodeId"/> allowing to find its corresponding data in the other data structures.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Main constructor for a new <see cref="NodeId"/> instance.
        /// </summary>
        /// <param name="id">Identifier of the node corresponding to its respective index in the <see cref="BoundingVolumeHierarchy{T}"/> structure.</param>
        public NodeId(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Returns a string that represents the current <see cref="NodeId"/>.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"NodeId({Id})";
        }

        /// <summary>
        /// This ID is reserved for invalid note handles. It points to no node in the BVH
        /// </summary>
        public static NodeId Null { get { return new NodeId(NullID); } }

        /// <summary>
        /// Get if the current node point to <see cref="NullID"/>.
        /// </summary>
        public bool IsNull { get { return Id == NullID; } }

        /// <summary>
        /// Node Id reserved for invalid nodes or null handles.
        /// </summary>
        public const int NullID = -1;


        public static bool operator ==(NodeId a, NodeId b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(NodeId a, NodeId b)
        {
            return a.Id != b.Id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NodeId))
                return false;

            return Equals((NodeId)obj);
        }

        public bool Equals(NodeId nodeId)
        {
            return Id == nodeId.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
