using System;

namespace Unity.Geospatial.Streaming
{
    public readonly struct MaterialType : IEquatable<MaterialType>
    {
        /// <summary>
        /// Determine if the material is lit or unlit
        /// </summary>
        public readonly MaterialLighting Lighting;

        /// <summary>
        /// Determine if the material is opaque, alpha tested, or transparent
        /// </summary>
        public readonly MaterialAlphaMode AlphaMode;

        public MaterialType(MaterialLighting lighting, MaterialAlphaMode alphaMode)
        {
            Lighting = lighting;
            AlphaMode = alphaMode;
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialType type && Equals(type);
        }

        public bool Equals(MaterialType obj)
        {
            return Lighting == obj.Lighting && AlphaMode == obj.AlphaMode;
        }

        public override int GetHashCode()
        {
            return (Lighting.GetHashCode()) ^ (397 * AlphaMode.GetHashCode());
        }
    }
}
