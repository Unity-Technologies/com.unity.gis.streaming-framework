using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    public struct MaterialProperty
    {
        public MaterialProperty(
            MaterialPropertyType type,
            MaterialPropertyValue valueType,
            Color colorValue,
            float floatValue,
            Vector4 vectorValue,
            TextureID textureId,
            Texture2D texture,
            Rect multiTextureMask)
        {
            Type = type;
            ValueType = valueType;
            ColorValue = colorValue;
            FloatValue = floatValue;
            VectorValue = vectorValue;
            TextureId = textureId;
            Texture = texture;
            MultiTextureMask = multiTextureMask;
        }

        public MaterialPropertyType Type { get; set; }

        public MaterialPropertyValue ValueType { get; set; }

        public Color ColorValue { get; set; }

        public float FloatValue { get; set; }

        public Vector4 VectorValue { get; set; }

        public TextureID TextureId { get; set; }

        public Texture2D Texture { get; set; }

        public Rect MultiTextureMask { get; set; }

        public static MaterialProperty Color(Color color)
        {
            return new MaterialProperty
            (
                MaterialPropertyType.ColorValue,
                MaterialPropertyValue.Color,

                color,
                default,
                default,

                default,
                default,

                default
            );
        }

        public static MaterialProperty AlbedoTexture(TextureID texture)
        {
            return AlbedoTexture(texture, new Vector4(1, 1, 0, 0));
        }

        public static MaterialProperty AlbedoTexture(TextureID texture, Vector4 textureST)
        {
            return new MaterialProperty
            (
                MaterialPropertyType.ColorTexture,
                MaterialPropertyValue.Texture,

                default,
                default,
                textureST,

                texture,
                null,

                default
            );
        }

        public static MaterialProperty AlbedoTerrain(TextureID texture, Rect mask, Vector2 translation, Vector2 scale)
        {
            return new MaterialProperty
            (
                MaterialPropertyType.ColorTexture,
                MaterialPropertyValue.TerrainTexture,

                default,
                default,
                new Vector4(scale.x, scale.y, translation.x, translation.y),

                texture,
                null,
                mask
            );
        }

        public static MaterialProperty Smoothness(float smoothness)
        {
            return new MaterialProperty
            (
                MaterialPropertyType.Smoothness,
                MaterialPropertyValue.Float,

                default,
                smoothness,
                default,

                default,
                null,

                default
            );
        }

        /// <summary>
        /// Set alpha cutoff value for alpha-test when the alpha-test mode is enabled
        /// This value is ignored if alpha-test mode is disabled
        /// </summary>
        /// <param name="alphaCutoff">
        /// If the alpha value is greater than or equal to this value, then it is rendered as fully opaque, otherwise, it is rendered as fully transparent.
        /// </param>
        /// <returns></returns>
        public static MaterialProperty AlphaCutoff(float alphaCutoff)
        {
            return new MaterialProperty
            (
                MaterialPropertyType.AlphaCutoff,
                MaterialPropertyValue.Float,

                default,
                alphaCutoff,
                default,

                default,
                null,

                default
            );
        }
    }
}
