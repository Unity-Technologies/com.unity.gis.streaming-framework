
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Unique identifier of a <see cref="UGDataSource"/>.
    /// This allow indirection to the <see cref="UGDataSource"/> and keep the link even after the
    /// <see cref="UGDataSource"/> as been disposed of.
    /// </summary>
    public readonly struct UGDataSourceID
    {
        /// <summary>
        /// Universal unique identifier of this data source.
        /// </summary>
        public readonly long Uuid;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="uuid">Universal unique identifier of the data source.</param>
        public UGDataSourceID(long uuid)
        {
            Uuid = uuid;
        }

        /// <summary>
        /// Cast an <see cref="UGDataSourceID"/> to a <see langword="long"/>.
        /// </summary>
        /// <param name="id">Instance to be converted.</param>
        /// <returns>The <see langword="long"/> representation of the given data source.</returns>
        public static explicit operator long(UGDataSourceID id)
        {
            return id.Uuid;
        }

        /// <summary>
        /// Cast a <see langword="long"/> to a <see cref="UGDataSourceID"/>.
        /// </summary>
        /// <param name="uuid">Long value to be converted.</param>
        /// <returns>The <see cref="UGDataSourceID"/> representation of the given instance.</returns>
        public static explicit operator UGDataSourceID(long uuid)
        {
            return new UGDataSourceID(uuid);
        }

        /// <summary>
        /// Get the string representation of this <see cref="UGDataSourceID"/>.
        /// </summary>
        /// <returns>The string result.</returns>
        public override string ToString()
        {
            return $"DataSourceID:{Uuid}";
        }

        /// <summary>
        /// Get if two <see cref="UGDataSourceID"/> represent the same.
        /// </summary>
        /// <param name="other">Compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(UGDataSourceID other)
        {
            return Uuid == other.Uuid;
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
            return (obj is UGDataSourceID o) && Equals(o);
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
            return Uuid.GetHashCode();
        }

        /// <summary>
        /// Get if two <see cref="UGDataSourceID"/> represent the same.
        /// </summary>
        /// <param name="first">Compare with this instance.</param>
        /// <param name="second">Compare the <paramref name="first"/> instance with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(UGDataSourceID first, UGDataSourceID second)
        {
            return first.Uuid == second.Uuid;
        }

        /// <summary>
        /// Get if two <see cref="UGDataSourceID"/> does not represent the same.
        /// </summary>
        /// <param name="first">Compare with this instance.</param>
        /// <param name="second">Compare the <paramref name="first"/> instance with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are not the same;
        /// <see langword="false"/> if both instances are the same.
        /// </returns>
        public static bool operator !=(UGDataSourceID first, UGDataSourceID second)
        {
            return first.Uuid != second.Uuid;
        }

        /// <summary>
        /// Get a <see cref="UGDataSourceID"/> representing a null value.
        /// </summary>
        public static UGDataSourceID Null
        {
            get { return (UGDataSourceID)0; }
        }
    }
}
