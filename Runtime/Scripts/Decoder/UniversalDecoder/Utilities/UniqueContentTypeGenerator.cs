
namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Unique identifier generator used for <see cref="ContentType"/> registration.
    /// </summary>
    public class UniqueContentTypeGenerator
    {
        private int m_CurrentId = ContentType.UnreservedStart;

        /// <summary>
        /// Create a new <see cref="ContentType"/>.
        /// </summary>
        /// <returns>The newly generated instance.</returns>
        public ContentType Generate()
        {
            return new ContentType(m_CurrentId++);
        }
        
    }
}
