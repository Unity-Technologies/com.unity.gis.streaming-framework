namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Unique identifier of a <see href="https://docs.unity3d.com/ScriptReference/Texture.html">Texture</see>.
    /// This allow indirection to the <see href="https://docs.unity3d.com/ScriptReference/Texture.html">Texture</see>
    /// and keep the link even after the <see href="https://docs.unity3d.com/ScriptReference/Texture.html">Texture</see>
    /// as been disposed of.
    /// </summary>
    public readonly struct TextureID
    {
        /// <summary>
        /// Universal unique identifier of this texture.
        /// </summary>
        private readonly long m_Uuid;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="uuid">Universal unique identifier of the texture.</param>
        public TextureID(long uuid)
        {
            m_Uuid = uuid;
        }

        /// <summary>
        /// Cast an <see cref="TextureID"/> to a <see langword="long"/>.
        /// </summary>
        /// <param name="id">Instance to be converted.</param>
        /// <returns>The <see langword="long"/> representation of the given texture.</returns>
        public static explicit operator long(TextureID id)
        {
            return id.m_Uuid;
        }

        /// <summary>
        /// Cast a <see langword="long"/> to a <see cref="TextureID"/>.
        /// </summary>
        /// <param name="uuid">Long value to be converted.</param>
        /// <returns>The <see cref="TextureID"/> representation of the given instance.</returns>
        public static explicit operator TextureID(long uuid)
        {
            return new TextureID(uuid);
        }

        /// <summary>
        /// Get the string representation of this <see cref="TextureID"/>.
        /// </summary>
        /// <returns>The string result.</returns>
        public override string ToString()
        {
            return $"TextureID: {m_Uuid}";
        }

        /// <summary>
        /// Get if two <see cref="TextureID"/> represent the same.
        /// </summary>
        /// <param name="other">Compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(TextureID other)
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
            return (obj is TextureID o) && Equals(o);
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
        /// Get if two <see cref="TextureID"/> represent the same.
        /// </summary>
        /// <param name="first">Compare with this first instance.</param>
        /// <param name="second">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(TextureID first, TextureID second)
        {
            return first.m_Uuid == second.m_Uuid;
        }

        /// <summary>
        /// Get if two <see cref="TextureID"/> does not represent the same.
        /// </summary>
        /// <param name="first">Compare with this first instance.</param>
        /// <param name="second">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are not the same;
        /// <see langword="false"/> if both instances are the same.
        /// </returns>
        public static bool operator !=(TextureID first, TextureID second)
        {
            return first.m_Uuid != second.m_Uuid;
        }
        
        /// <summary>
        /// Get a <see cref="TextureID"/> representing a null value.
        /// </summary>
        public static TextureID Null
        {
            get { return (TextureID)0; }
        }
    }
}
