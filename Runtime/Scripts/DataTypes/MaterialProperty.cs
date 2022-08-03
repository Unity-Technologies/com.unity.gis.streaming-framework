using UnityEngine;

namespace Unity.Geospatial.Streaming
{ 
    /// <summary>
    /// Apply a set of values to the material this property is attached to.
    /// This can be used to either change the color of the material, apply a texture, set the transparency and many more.
    /// <remarks>Some <see cref="Type"/> / <see cref="ValueType"/> can only be applied once per material.</remarks>
    /// </summary>
    public struct MaterialProperty
    {
        /// <summary>
        /// Constructor for a color property.
        /// </summary>
        /// <param name="type">Specify how a <see cref="MaterialProperty">property</see> affects the
        /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.</param>
        /// <param name="colorValue">Set the <see cref="ColorValue"/>.</param>
        public MaterialProperty(
            MaterialPropertyType type,
            Color colorValue)
        {
            Type = type;
            ValueType = MaterialPropertyValue.Color;
            ColorValue = colorValue;
            FloatValue = default;
            Vector4Value = default;
            TextureId = default;
            Texture = null;
            MultiTextureMask = default;
        }

        /// <summary>
        /// Constructor for a texture property.
        /// </summary>
        /// <param name="type">Specify how a <see cref="MaterialProperty">property</see> affects the
        /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.</param>
        /// <param name="valueType">Specify which elements of the <see cref="MaterialProperty"/> to apply
        /// on the <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.</param>
        /// <param name="textureSt">Scale / Transform the texture where x/y represent the scale and z/w represent the position.</param>
        /// <param name="textureId">Apply this texture to the material.</param>
        /// <param name="multiTextureMask">Apply the given <paramref name="textureId">texture</paramref> to this area.</param>
        public MaterialProperty(
            MaterialPropertyType type,
            MaterialPropertyValue valueType,
            Vector4 textureSt,
            TextureID textureId,
            Rect multiTextureMask = default)
        {
            Type = type;
            ValueType = valueType;
            ColorValue = default;
            FloatValue = default;
            Vector4Value = textureSt;
            TextureId = textureId;
            Texture = null;
            MultiTextureMask = multiTextureMask;
        }
        
        /// <summary>
        /// Constructor for a float property.
        /// </summary>
        /// <param name="type">Specify how a <see cref="MaterialProperty">property</see> affects the
        /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.</param>
        /// <param name="floatValue">Set the <see cref="FloatValue"/>.</param>
        public MaterialProperty(
            MaterialPropertyType type,
            float floatValue)
        {
            Type = type;
            ValueType = MaterialPropertyValue.Float;
            ColorValue = default;
            FloatValue = floatValue;
            Vector4Value = default;
            TextureId = default;
            Texture = null;
            MultiTextureMask = default;
        }

        /// <summary>
        /// Specify how a <see cref="MaterialProperty">property</see> affects the
        /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.
        /// </summary>
        public MaterialPropertyType Type { get; set; }

        /// <summary>
        /// Specify which elements of the <see cref="MaterialProperty"/> to apply
        /// on the <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.
        /// </summary>
        public MaterialPropertyValue ValueType { get; set; }

        /// <summary>
        /// Set a color value based on the <see cref="Type"/>.
        /// </summary>
        public Color ColorValue { get; set; }

        /// <summary>
        /// Set a float value based on the <see cref="Type"/>.
        /// </summary>
        public float FloatValue { get; set; }

        /// <summary>
        /// Set a Vector4 value based on the <see cref="Type"/>.
        /// </summary>
        public Vector4 Vector4Value { get; set; }

        /// <summary>
        /// Apply a <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture</see> value based on the <see cref="Type"/>.
        /// </summary>
        public TextureID TextureId { get; set; }

        /// <summary>
        /// Apply a <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture</see> value based on the <see cref="Type"/>.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Apply the <see cref="TextureId">texture</see> to this area.
        /// </summary>
        public Rect MultiTextureMask { get; set; }

        /// <summary>
        /// Create a new <see cref="MaterialProperty"/> with a color property used for the albedo channel.
        /// </summary>
        /// <param name="color">Set the <see cref="ColorValue"/>.</param>
        /// <returns>The newly created <see cref="MaterialProperty"/> instance.</returns>
        public static MaterialProperty AlbedoColor(Color color)
        {
            return new MaterialProperty
            (
                MaterialPropertyType.AlbedoColor,
                color
            );
        }

        /// <summary>
        /// Create a new <see cref="MaterialProperty"/> with a texture property used for the albedo channel.
        /// </summary>
        /// <param name="texture">Apply this texture to the material.</param>
        /// <returns>The newly created <see cref="MaterialProperty"/> instance.</returns>
        public static MaterialProperty AlbedoTexture(TextureID texture)
        {
            return AlbedoTexture(texture, new Color(1, 1, 0, 0));
        }

        /// <summary>
        /// Create a new <see cref="MaterialProperty"/> with a texture property used for the albedo channel.
        /// </summary>
        /// <param name="texture">Apply this texture to the material.</param>
        /// <param name="textureSt">Multiply the given <paramref name="texture"/> by this value.</param>
        /// <returns>The newly created <see cref="MaterialProperty"/> instance.</returns>
        public static MaterialProperty AlbedoTexture(TextureID texture, Color textureSt)
        {
            return new MaterialProperty
            (
                MaterialPropertyType.AlbedoTexture,
                MaterialPropertyValue.Texture,
                textureSt,

                texture
            );
        }

        /// <summary>
        /// Create a new <see cref="MaterialProperty"/> with a texture property used as a multi-textured terrain.
        /// </summary>
        /// <param name="texture">Apply this texture to the material.</param>
        /// <param name="mask">Apply the given <paramref name="texture"/> to this area.</param>
        /// <param name="translation">Position in uv space of the texture.</param>
        /// <param name="scale">Size of the texture in percentage where 1.0 represent 100%.</param>
        /// <returns>The newly created <see cref="MaterialProperty"/> instance.</returns>
        public static MaterialProperty AlbedoTerrain(TextureID texture, Rect mask, Vector2 translation, Vector2 scale)
        {
            return new MaterialProperty
            (
                MaterialPropertyType.AlbedoTexture,
                MaterialPropertyValue.TerrainTexture,

                new Color(scale.x, scale.y, translation.x, translation.y),

                texture,
                mask
            );
        }

        /// <summary>
        /// Create a new <see cref="MaterialProperty"/> applied to the
        /// <see href="https://docs.unity3d.com/Manual/StandardShaderMaterialParameterSmoothness.html">smoothness</see>
        /// of the material.
        /// </summary>
        /// <param name="smoothness">Set the <see cref="FloatValue"/>.</param>
        /// <returns>The newly created <see cref="MaterialProperty"/> instance.</returns>
        public static MaterialProperty Smoothness(float smoothness)
        {
            return new MaterialProperty
            (
                MaterialPropertyType.Smoothness,
                smoothness
            );
        }

        /// <summary>
        /// Set alpha cutoff value for alpha-test when the alpha-test mode is enabled
        /// This value is ignored if alpha-test mode is disabled
        /// </summary>
        /// <param name="alphaCutoff">
        /// If the alpha value is greater than or equal to this value, then it is rendered as fully opaque, otherwise, it is rendered as fully transparent.
        /// </param>
        /// <returns>The newly created <see cref="MaterialProperty"/> instance.</returns>
        public static MaterialProperty AlphaCutoff(float alphaCutoff)
        {
            return new MaterialProperty
            (
                MaterialPropertyType.AlphaCutoff,
                alphaCutoff
            );
        }
    }
}
