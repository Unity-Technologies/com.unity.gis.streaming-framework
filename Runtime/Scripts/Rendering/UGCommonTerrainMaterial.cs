using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Multi-Material allowing apply multiple
    /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">Materials</see> / <see href="https://docs.unity3d.com/ScriptReference/Texture.html">Textures</see>
    /// to the same <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
    /// </summary>
    public class UGCommonTerrainMaterial : UGMaterial
    {
        /// <summary>
        /// <see cref="UGCommonTerrainMaterial"/> creator with two layers (<see cref="BaseShader"/>, <see cref="OverlayShader"/>).
        /// </summary>
        public readonly struct Factory
        {
            /// <summary>
            /// Name of the underlying shader.
            /// </summary>
            public string BaseShader { get; }

            /// <summary>
            /// Name of the overlying shader.
            /// </summary>
            public string OverlayShader { get; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="baseShader">Name of the underlying shader.</param>
            /// <param name="overlayShader">Name of the overlying shader.</param>
            public Factory(string baseShader, string overlayShader)
            {
                BaseShader = baseShader;
                OverlayShader = overlayShader;
            }

            /// <summary>
            /// Create a new <see cref="UGMaterial"/> instance based on the shaders of this <see cref="Factory"/>.
            /// </summary>
            /// <returns></returns>
            public UGMaterial Instantiate()
            {
                return new UGCommonTerrainMaterial(BaseShader, OverlayShader);
            }
        }

        private struct ShaderProperties
        {
            public int texture;
            public int mask;
            public int texture_st;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="baseShader">Name of the underlying shader.</param>
        /// <param name="overlayShader">Name of the overlying shader.</param>
        public UGCommonTerrainMaterial(string baseShader, string overlayShader) :
            base(isComposite: true)
        {
            if (s_BaseShader == null)
                s_BaseShader = Shader.Find(baseShader);

            if (s_OverlayShader == null)
                s_OverlayShader = Shader.Find(overlayShader);

            IncrementMaterialCount();
        }

        /// <summary>
        /// High Definition Render Pipeline <see cref="Factory"/> with no lighting contribution.
        /// </summary>
        public static readonly Factory HDRPUnlit = new Factory("Geospatial/SRP/UnlitTerrainBase", "Geospatial/SRP/UnlitTerrainOverlay");

        /// <summary>
        /// High Definition Render Pipeline <see cref="Factory"/> with lighting contribution.
        /// </summary>
        public static readonly Factory HDRPLit = new Factory("Geospatial/SRP/LitTerrainBase", "Geospatial/SRP/LitTerrainOverlay");

        /// <summary>
        /// Universal Render Pipeline <see cref="Factory"/> with no lighting contribution.
        /// </summary>
        public static readonly Factory URPUnlit = new Factory("Geospatial/SRP/UnlitTerrainBase", "Geospatial/SRP/UnlitTerrainOverlay");

        /// <summary>
        /// Universal Render Pipeline <see cref="Factory"/> with lighting contribution.
        /// </summary>
        public static readonly Factory URPLit = new Factory("Geospatial/SRP/LitTerrainBase", "Geospatial/SRP/LitTerrainOverlay");

        /// <summary>
        /// Built-in Render Pipeline <see cref="Factory"/> with no lighting contribution.
        /// </summary>
        public static readonly Factory BuiltinUnlit = new Factory("Geospatial/Builtin/UnlitTerrain", "Geospatial/Builtin/UnlitTerrain");

        private const float k_MaskEpsilon = 0.001f;
        private static Shader s_BaseShader;
        private static Shader s_OverlayShader;
        private readonly List<Material> m_Materials = new List<Material>();

        private int m_OccupiedSlots = 0;
        private readonly List<Texture2D> m_Slots = new List<Texture2D>(4);
        private static readonly List<ShaderProperties> k_ShaderProperties = new List<ShaderProperties>
        {
            new ShaderProperties
            {
                texture =       Shader.PropertyToID("_ATexture"),
                mask =          Shader.PropertyToID("_ATexture_Mask"),
                texture_st =   Shader.PropertyToID("_ATexture_ScaleOffset")
            },
            new ShaderProperties
            {
                texture =       Shader.PropertyToID("_BTexture"),
                mask =          Shader.PropertyToID("_BTexture_Mask"),
                texture_st =   Shader.PropertyToID("_BTexture_ScaleOffset")
            },
            new ShaderProperties
            {
                texture =       Shader.PropertyToID("_CTexture"),
                mask =          Shader.PropertyToID("_CTexture_Mask"),
                texture_st =   Shader.PropertyToID("_CTexture_ScaleOffset")
            },
            new ShaderProperties
            {
                texture =       Shader.PropertyToID("_DTexture"),
                mask =          Shader.PropertyToID("_DTexture_Mask"),
                texture_st =   Shader.PropertyToID("_DTexture_ScaleOffset")
            },
        };

        /// <inheritdoc cref="UGMaterial.UnityMaterials"/>
        public override List<Material> UnityMaterials
        {
            get
            {
                return m_Materials;
            }
        }

        /// <inheritdoc cref="UGMaterial.OnDispose"/>
        protected override void OnDispose()
        {
            foreach (Material material in m_Materials)
            {
                if (Application.isPlaying)
                    Object.Destroy(material);
                else
                    Object.DestroyImmediate(material);
            }
        }

        private void IncrementMaterialCount()
        {
            int start = m_Slots.Count;
            int end = start + 4;

            Material material = new Material(m_Materials.Count == 0 ? s_BaseShader : s_OverlayShader);

            //
            //  TODO - Implement default color scheme
            //
#if UNITY_GEOSPATIAL_DEBUG
            material.SetColor("_DefaultColor", Color.magenta);
#endif
            m_Materials.Add(material);
            for (int i = start; i < end; i++)
                m_Slots.Add(null);
        }

        private void AdjustMask(ref MaterialProperty cmp)
        {
            Rect v = cmp.MultiTextureMask;

            if (v.x + v.width > 1.0f - k_MaskEpsilon)
                v.width += 0.1f;
            if (v.y + v.height > 1.0f - k_MaskEpsilon)
                v.height += 0.1f;
            if (v.x < k_MaskEpsilon)
            {
                v.x -= 0.1f;
                v.width += 0.1f;
            }
            if (v.y < k_MaskEpsilon)
            {
                v.y -= 0.1f;
                v.height += 0.1f;
            }


            cmp.MultiTextureMask = v;
        }

        /// <inheritdoc cref="UGMaterial.OnAddMaterialProperty"/>
        protected override void OnAddMaterialProperty(MaterialProperty materialProperty)
        {
            AdjustMask(ref materialProperty);

            //
            //  TODO - Patchy implementation, rework
            //
            switch (materialProperty.Type)
            {
                case MaterialPropertyType.AlbedoTexture:

                    Assert.IsNotNull(materialProperty.Texture);
                    materialProperty.Texture.wrapMode = TextureWrapMode.Clamp;

                    if (m_OccupiedSlots >= m_Slots.Count)
                        IncrementMaterialCount();

                    for (int i = 0; i < m_Slots.Count; i++)
                    {
                        if (m_Slots[i] != null)
                            continue;

                        m_Slots[i] = materialProperty.Texture;

                        Rect mask = materialProperty.MultiTextureMask;
                        m_Materials[i / 4].SetTexture(k_ShaderProperties[i % 4].texture, materialProperty.Texture);
                        m_Materials[i / 4].SetVector(k_ShaderProperties[i % 4].mask, new Vector4(mask.x, mask.y, mask.width, mask.height));
                        m_Materials[i / 4].SetVector(k_ShaderProperties[i % 4].texture_st, materialProperty.Vector4Value);

                        m_OccupiedSlots++;

                        return;
                    }
                    break;
            }


            Debug.LogErrorFormat("Attempting to add more than {0} textures", m_Slots.Count);
        }

        /// <inheritdoc cref="UGMaterial.OnRemoveMaterialProperty"/>
        protected override void OnRemoveMaterialProperty(MaterialProperty materialProperty)
        {
            //
            //  TODO - Manage other types of material components
            //
            Assert.IsNotNull(materialProperty.Texture);
            Assert.AreEqual(MaterialPropertyType.AlbedoTexture, materialProperty.Type);

            for (int i = 0; i < m_Slots.Count; i++)
            {
                if (m_Slots[i] != materialProperty.Texture)
                    continue;

                m_Slots[i] = null;
                m_Materials[i / 4].SetTexture(k_ShaderProperties[i % 4].texture, null);

                m_OccupiedSlots--;

                return;
            }

            Debug.LogError("Could not find texture to remove");
        }
    }
}
