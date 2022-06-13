
namespace Unity.Geospatial.Streaming
{
    public readonly struct MeshID
    {
        private readonly long m_Uuid;

        public MeshID(long uuid)
        {
            m_Uuid = uuid;
        }

        public static explicit operator long(MeshID id)
        {
            return id.m_Uuid;
        }

        public static explicit operator MeshID(long uuid)
        {
            return new MeshID(uuid);
        }

        public override string ToString()
        {
            return $"MeshID: {m_Uuid}";
        }

        public bool Equals(MeshID other)
        {
            return m_Uuid == other.m_Uuid;
        }

        public override bool Equals(object obj)
        {
            return (obj is MeshID o) && Equals(o);
        }

        public override int GetHashCode()
        {
            return m_Uuid.GetHashCode();
        }

        public static bool operator ==(MeshID first, MeshID second)
        {
            return first.m_Uuid == second.m_Uuid;
        }

        public static bool operator !=(MeshID first, MeshID second)
        {
            return first.m_Uuid != second.m_Uuid;
        }

        public static MeshID Null
        {
            get
            {
                return (MeshID)0;
            }
        }
    }
}
