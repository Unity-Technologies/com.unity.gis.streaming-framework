
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The UUID Generator is used to generate session unique identifiers for
    /// various geospatial internals. It is thread safe, ensuring that it can
    /// be accessed both from the main thread as well as from any other thread
    /// without worrying about thread safety.
    /// </summary>
    public class UUIDGenerator
    {
        private long m_Current = 10;
        
        private static readonly object s_Lock = new object();

        public static UUIDGenerator Instance { get; } = new UUIDGenerator();

        private UUIDGenerator() { }
        
        /// <summary>
        /// Generate a new InstanceID
        /// </summary>
        /// <returns>InstanceID</returns>
        public InstanceID GetInstanceID()
        {
            lock (s_Lock)
                return (InstanceID)m_Current++;
        }

        /// <summary>
        /// Generate a new Material ID
        /// </summary>
        /// <returns>MaterialID</returns>
        public MaterialID GetMaterialID()
        {
            lock (s_Lock)
                return (MaterialID)m_Current++;
        }

        /// <summary>
        /// Generate a new Texture ID
        /// </summary>
        /// <returns>TextureID</returns>
        public TextureID GetTextureID()
        {
            lock (s_Lock)
                return (TextureID)m_Current++;
        }

        /// <summary>
        /// Generate a new Mesh ID
        /// </summary>
        /// <returns>MeshID</returns>
        public MeshID GetMeshID()
        {
            lock (s_Lock)
                return (MeshID)m_Current++;
        }
    }

}
