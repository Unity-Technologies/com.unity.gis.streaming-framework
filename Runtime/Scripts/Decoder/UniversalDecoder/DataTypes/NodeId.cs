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
        public static NodeId Null
        {
            get { return new NodeId(NullID); }
        }

        /// <summary>
        /// Get if the current node point to <see cref="NullID"/>.
        /// </summary>
        public bool IsNull
        {
            get { return Id == NullID; }
        }

        /// <summary>
        /// Node Id reserved for invalid nodes or null handles.
        /// </summary>
        public const int NullID = -1;


        /// <summary>
        /// Validate both <see cref="NodeId"/> have the same values.
        /// </summary>
        /// <param name="lhs">First instance to compare with.</param>
        /// <param name="rhs">Compare <paramref name="lhs"/> with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same id;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(NodeId lhs, NodeId rhs)
        {
            return lhs.Id == rhs.Id;
        }

        /// <summary>
        /// Validate both <see cref="NodeId"/> have the different values.
        /// </summary>
        /// <param name="lhs">First instance to compare with.</param>
        /// <param name="rhs">Compare <paramref name="lhs"/> with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the different id;
        /// <see langword="false"/> if both instances represent the same id.
        /// </returns>
        public static bool operator !=(NodeId lhs, NodeId rhs)
        {
            return lhs.Id != rhs.Id;
        }

        /// <summary>
        /// Validate <paramref name="obj"/> is a <see cref="NodeId"/> instance and is the same id.
        /// </summary>
        /// <param name="obj">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same id;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is NodeId id))
                return false;

            return Equals(id);
        }

        /// <summary>
        /// Validate an other <see cref="NodeId"/> is the same id.
        /// </summary>
        /// <param name="other">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same id;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(NodeId other)
        {
            return Id == other.Id;
        }

        /// <summary>
        /// Compute a hash code for the object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <remarks>
        /// * You should not assume that equal hash codes imply object equality.
        /// * You should never persist or use a hash code outside the application domain in which it was created,
        ///   because the same object may hash differently across application domains, processes, and platforms.
        /// </remarks>
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
