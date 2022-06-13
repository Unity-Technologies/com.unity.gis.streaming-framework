
namespace Unity.Geospatial.Streaming
{
    public readonly struct MaterialID
    {
        private readonly long m_Uuid;

        public MaterialID(long uuid)
        {
            m_Uuid = uuid;
        }

        public static explicit operator long(MaterialID id)
        {
            return id.m_Uuid;
        }

        public static explicit operator MaterialID(long uuid)
        {
            return new MaterialID(uuid);
        }

        public override string ToString()
        {
            return $"MaterialID: {m_Uuid}";
        }

        public bool Equals(MaterialID other)
        {
            return m_Uuid == other.m_Uuid;
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialID o && Equals(o);
        }

        public override int GetHashCode()
        {
            return m_Uuid.GetHashCode();
        }

        public static bool operator ==(MaterialID first, MaterialID second)
        {
            return first.m_Uuid == second.m_Uuid;
        }

        public static bool operator !=(MaterialID first, MaterialID second)
        {
            return first.m_Uuid != second.m_Uuid;
        }
    }
}
