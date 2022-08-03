
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Unique identifier of an instance (Transform for a given geometry with a specified material).
    /// </summary>
    public readonly struct InstanceID
    {
        /// <summary>
        /// Universal unique identifier of this instance.
        /// </summary>
        private readonly long m_Uuid;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="uuid">Universal unique identifier of the instance.</param>
        public InstanceID(long uuid)
        {
            m_Uuid = uuid;
        }

        /// <summary>
        /// Cast an <see cref="InstanceID"/> to a <see langword="long"/>.
        /// </summary>
        /// <param name="instanceId">Instance to be converted.</param>
        /// <returns>The <see langword="long"/> representation of the given instance.</returns>
        public static explicit operator long(InstanceID instanceId)
        {
            return instanceId.m_Uuid;
        }

        /// <summary>
        /// Cast a <see langword="long"/> to a <see cref="InstanceID"/>.
        /// </summary>
        /// <param name="uuid">Long value to be converted.</param>
        /// <returns>The <see cref="InstanceID"/> representation of the given instance.</returns>
        public static explicit operator InstanceID(long uuid)
        {
            return new InstanceID(uuid);
        }

        /// <summary>
        /// Get the string representation of this <see cref="InstanceID"/>.
        /// </summary>
        /// <returns>The string result.</returns>
        public override string ToString()
        {
            return $"InstanceID: {m_Uuid}";
        }

        /// <summary>
        /// Get if two <see cref="InstanceID"/> represent the same.
        /// </summary>
        /// <param name="other">Compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(InstanceID other)
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
            return obj is InstanceID o && Equals(o);
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
        /// Get if two <see cref="InstanceID"/> represent the same.
        /// </summary>
        /// <param name="first">Compare with this first instance.</param>
        /// <param name="second">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(InstanceID first, InstanceID second)
        {
            return first.m_Uuid == second.m_Uuid;
        }

        /// <summary>
        /// Get if two <see cref="InstanceID"/> does not represent the same.
        /// </summary>
        /// <param name="first">Compare with this first instance.</param>
        /// <param name="second">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are not the same;
        /// <see langword="false"/> if both instances are the same.
        /// </returns>
        public static bool operator !=(InstanceID first, InstanceID second)
        {
            return first.m_Uuid != second.m_Uuid;
        }

        /// <summary>
        /// Get an <see cref="InstanceID"/> representing a null value.
        /// </summary>
        public static InstanceID Null
        {
            get
            {
                return new InstanceID(0);
            }
        }
    }
}
