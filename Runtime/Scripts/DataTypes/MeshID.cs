
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Unique identifier of a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
    /// This allow indirection to the <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>
    /// and keep the link even after the <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>
    /// as been disposed of.
    /// </summary>
    public readonly struct MeshID
    {
        /// <summary>
        /// Universal unique identifier of this mesh.
        /// </summary>
        private readonly long m_Uuid;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="uuid">Universal unique identifier of the mesh.</param>
        public MeshID(long uuid)
        {
            m_Uuid = uuid;
        }

        /// <summary>
        /// Cast an <see cref="MeshID"/> to a <see langword="long"/>.
        /// </summary>
        /// <param name="id">Instance to be converted.</param>
        /// <returns>The <see langword="long"/> representation of the given mesh.</returns>
        public static explicit operator long(MeshID id)
        {
            return id.m_Uuid;
        }

        /// <summary>
        /// Cast a <see langword="long"/> to a <see cref="MeshID"/>.
        /// </summary>
        /// <param name="uuid">Long value to be converted.</param>
        /// <returns>The <see cref="MeshID"/> representation of the given instance.</returns>
        public static explicit operator MeshID(long uuid)
        {
            return new MeshID(uuid);
        }

        /// <summary>
        /// Get the string representation of this <see cref="MeshID"/>.
        /// </summary>
        /// <returns>The string result.</returns>
        public override string ToString()
        {
            return $"MeshID: {m_Uuid}";
        }

        /// <summary>
        /// Get if two <see cref="MeshID"/> represent the same.
        /// </summary>
        /// <param name="other">Compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(MeshID other)
        {
            return m_Uuid == other.m_Uuid;
        }

        /// <summary>
        /// Get if this instance is the same as the given <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is MeshID o) && Equals(o);
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
            return m_Uuid.GetHashCode();
        }

        /// <summary>
        /// Get if two <see cref="MeshID"/> represent the same.
        /// </summary>
        /// <param name="first">Compare with this first instance.</param>
        /// <param name="second">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(MeshID first, MeshID second)
        {
            return first.m_Uuid == second.m_Uuid;
        }

        /// <summary>
        /// Get if two <see cref="MeshID"/> does not represent the same.
        /// </summary>
        /// <param name="first">Compare with this first instance.</param>
        /// <param name="second">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are not the same;
        /// <see langword="false"/> if both instances are the same.
        /// </returns>
        public static bool operator !=(MeshID first, MeshID second)
        {
            return first.m_Uuid != second.m_Uuid;
        }

        /// <summary>
        /// Get a <see cref="MeshID"/> representing a null value.
        /// </summary>
        public static MeshID Null
        {
            get
            {
                return (MeshID)0;
            }
        }
    }
}
