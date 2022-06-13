using System;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// This struct should only be used for serialization purposes within Unity, either
    /// in MonoBehaviours, such that it can appear in the inspector, or for JSON
    /// serialization. For any purpose that does not require serialization, please use
    /// <see cref="MaterialType"/>. Cast as quickly as you can to the MaterialType struct
    /// which is immutable.
    /// </summary>
    [Serializable]
    public struct SerializableMaterialType
    {
        /// <summary>
        /// Determine if the material is lit or unlit
        /// </summary>
        public MaterialLighting Lighting;

        /// <summary>
        /// Determine if the material is opaque, alpha tested, or transparent
        /// </summary>
        public MaterialAlphaMode AlphaMode;

        /// <summary>
        /// Default constructor, using a <see cref="MaterialType"/>.
        /// </summary>
        /// <param name="type">The type which should be copied into the serializable version</param>
        public SerializableMaterialType(MaterialType type)
        {
            Lighting = type.Lighting;
            AlphaMode = type.AlphaMode;
        }

        /// <summary>
        /// Cast to <see cref="MaterialType"/>
        /// </summary>
        /// <param name="serializableMaterialType">The <see cref="SerializableMaterialType"/> to be casted from</param>
        public static explicit operator MaterialType(SerializableMaterialType serializableMaterialType)
        {
            return new MaterialType(serializableMaterialType.Lighting, serializableMaterialType.AlphaMode);
        }

        /// <summary>
        /// Cast from <see cref="MaterialType"/>
        /// </summary>
        /// <param name="type">The <see cref="MaterialType"/> to be casted from</param>
        public static explicit operator SerializableMaterialType(MaterialType type)
        {
            return new SerializableMaterialType(type);
        }
    }
}
