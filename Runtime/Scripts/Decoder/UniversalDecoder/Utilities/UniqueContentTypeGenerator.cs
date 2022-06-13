
namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    public class UniqueContentTypeGenerator
    {
        private int m_CurrentId = ContentType.UnreservedStart;

        public ContentType Generate()
        {
            return new ContentType(m_CurrentId++);
        }
        
    }
}
