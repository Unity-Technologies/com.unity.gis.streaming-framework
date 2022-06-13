namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Interface used by the <see cref="UGObjectPool{T}"/> allowing indexing of the created objects.
    /// </summary>
    public interface IUGObject
    {
        /// <summary>
        /// <see langword="true"/> if the object is disposed and cannot be used;
        /// <see langword="false"/> otherwise.
        /// </summary>
        public bool Disposed { get; set; }
        
        /// <summary>
        /// Index value of the instance allowing faster retrieval within <see cref="UGObjectPool{T}"/>.
        /// </summary>
        public int Index { get; set; }
    }
}
