
namespace Unity.Geospatial.Streaming
{
    public struct InstanceGroupID
    {
        public readonly long Uuid;

        public InstanceGroupID(long uuid)
        {
            Uuid = uuid;
        }

        public static explicit operator long(InstanceGroupID id)
        {
            return id.Uuid;
        }

        public static explicit operator InstanceGroupID(long uuid)
        {
            return new InstanceGroupID(uuid);
        }
    }
}
