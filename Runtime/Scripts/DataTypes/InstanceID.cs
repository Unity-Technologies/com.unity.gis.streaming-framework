
namespace Unity.Geospatial.Streaming
{
    public readonly struct InstanceID
    {
        private readonly long m_Uuid;

        public InstanceID(long uuid)
        {
            m_Uuid = uuid;
        }

        public static explicit operator long(InstanceID id)
        {
            return id.m_Uuid;
        }

        public static explicit operator InstanceID(long uuid)
        {
            return new InstanceID(uuid);
        }

        public override string ToString()
        {
            return $"InstanceID: {m_Uuid}";
        }

        public bool Equals(InstanceID other)
        {
            return m_Uuid == other.m_Uuid;
        }

        public override bool Equals(object obj)
        {
            return obj is InstanceID o && Equals(o);
        }

        public override int GetHashCode()
        {
            return m_Uuid.GetHashCode();
        }

        public static bool operator ==(InstanceID first, InstanceID second)
        {
            return first.m_Uuid == second.m_Uuid;
        }

        public static bool operator !=(InstanceID first, InstanceID second)
        {
            return first.m_Uuid != second.m_Uuid;
        }

        public static InstanceID Null
        {
            get
            {
                return new InstanceID(0);
            }
        }
    }
}
