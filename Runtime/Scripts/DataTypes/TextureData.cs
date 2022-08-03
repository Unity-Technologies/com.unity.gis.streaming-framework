namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Use this <see langword="struct"/> when reading data that needs to be loaded
    /// as a <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture2D</see>.
    /// This <see langword="struct"/> is used to pass an image information to the <see cref="UGCommandBuffer"/>
    /// to be converted to a Unity <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture2D</see>.
    /// Unlike the <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">UnityEngine.Texture2D</see> class,
    /// this struct can be populated off of the main thread.
    /// </summary>
    public struct TextureData
    {
        /// <summary>
        /// Width of the Texture at full resolution in pixels.
        /// </summary>
        public int width;
        
        /// <summary>
        /// Height of the Texture at full resolution in pixels.
        /// </summary>
        public int height;
        
        /// <summary>
        /// Raw data array to initialize texture pixels with.
        /// </summary>
        public byte[] data;
    }
}
