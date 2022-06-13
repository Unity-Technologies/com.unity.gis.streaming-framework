namespace Unity.Geospatial.Streaming
{
    public readonly struct TextureID
    {
        private readonly long m_Uuid;

        public TextureID(long uuid)
        {
            m_Uuid = uuid;
        }

        public static TextureID Null
        {
            get { return (TextureID)0; }
        }

        public static explicit operator long(TextureID id)
        {
            return id.m_Uuid;
        }

        public static explicit operator TextureID(long uuid)
        {
            return new TextureID(uuid);
        }

        public override string ToString()
        {
            return $"TextureID: {m_Uuid}";
        }

        public bool Equals(TextureID other)
        {
            return m_Uuid == other.m_Uuid;
        }

        public override bool Equals(object obj)
        {
            return (obj is TextureID o) && Equals(o);
        }

        public override int GetHashCode()
        {
            return m_Uuid.GetHashCode();
        }

        public static bool operator ==(TextureID first, TextureID second)
        {
            return first.m_Uuid == second.m_Uuid;
        }

        public static bool operator !=(TextureID first, TextureID second)
        {
            return first.m_Uuid != second.m_Uuid;
        }


    }
}
