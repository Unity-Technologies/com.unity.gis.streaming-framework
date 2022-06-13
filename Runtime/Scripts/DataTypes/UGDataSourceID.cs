
namespace Unity.Geospatial.Streaming
{

    public readonly struct UGDataSourceID
    {
        public readonly long Uuid;

        public UGDataSourceID(long uuid)
        {
            Uuid = uuid;
        }

        public static UGDataSourceID Null
        {
            get => (UGDataSourceID)0;
        }

        public static explicit operator long(UGDataSourceID id)
        {
            return id.Uuid;
        }

        public static explicit operator UGDataSourceID(long uuid)
        {
            return new UGDataSourceID(uuid);
        }

        public override string ToString()
        {
            return $"DataSourceID:{Uuid}";
        }

        public bool Equals(UGDataSourceID other)
        {
            return Uuid == other.Uuid;
        }

        public override bool Equals(object obj)
        {
            return (obj is UGDataSourceID o) && Equals(o);
        }

        public override int GetHashCode()
        {
            return Uuid.GetHashCode();
        }

        public static bool operator ==(UGDataSourceID first, UGDataSourceID second)
        {
            return first.Uuid == second.Uuid;
        }

        public static bool operator !=(UGDataSourceID first, UGDataSourceID second)
        {
            return first.Uuid != second.Uuid;
        }
    }

}
