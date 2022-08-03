using System;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Property directive allowing the corresponding method call on a <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.
    /// </summary>
    [Serializable]
    public class UGShaderProperty
    {
        /// <summary>
        /// Specify how a <see cref="MaterialProperty">property</see> affects the
        /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.
        /// </summary>
        public MaterialPropertyType Type;
        
        /// <summary>
        /// Name of the property allowing a better understanding of its usage.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Texture transform / scale to apply.
        /// </summary>
        public string TextureScaleTiling;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="type">Specify how a <see cref="MaterialProperty">property</see> affects the
        /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.</param>
        /// <param name="name">Name of the property allowing a better understanding of its usage.</param>
        /// <param name="st">Texture transform / scale to apply.</param>
        public UGShaderProperty(MaterialPropertyType type, string name, string st = null)
        {
            Type = type;
            Name = name;
            TextureScaleTiling = st;
        }
    }
}
