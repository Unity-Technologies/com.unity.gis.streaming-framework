using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

using Object = UnityEngine.Object;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Renderer allowing multiple layers of materials for a single <see cref="Mesh"/>.
    /// For each <see cref="UGMaterial"/> part of the <see cref="Materials"/>, it will instantiate a new
    /// <see cref="Mesh"/>, apply the corresponding <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see>
    /// and output the Z Buffer only on the last instance. This allows to overlay multiple
    /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">Materials</see>.
    /// <remarks>This can be used when a single mesh is composed of multiple
    /// <see href="https://docs.unity3d.com/ScriptReference/Texture.html">textures</see> where each texture represent a
    /// sub-tile of the <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">mesh</see>.</remarks>
    /// </summary>
    public class GOCompositeRenderer : GOBaseRenderer
    {
        private sealed class SecondaryRenderer : IDisposable
        {
            public SecondaryRenderer()
            {
                m_GameObject = new GameObject("Composite Renderer");

                Transform transform = m_GameObject.transform;

                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;

                m_MeshFilter = m_GameObject.AddComponent<MeshFilter>();
                m_MeshRenderer = m_GameObject.AddComponent<MeshRenderer>();
            }

            private readonly GameObject m_GameObject;
            private readonly MeshFilter m_MeshFilter;
            private readonly MeshRenderer m_MeshRenderer;

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (Application.isPlaying)
                    Object.Destroy(m_GameObject);
                else
                    Object.DestroyImmediate(m_GameObject);
            }

            public int UnityLayer
            {
                set
                {
                    m_GameObject.layer = value;
                }
            }

            public Mesh Mesh
            {
                set
                {
                    m_MeshFilter.sharedMesh = value;
                }
            }

            public Material Material
            {
                set
                {
                    m_MeshRenderer.sharedMaterial = value;
                }
            }
        }

        /// <summary>
        /// Default constructor.
        /// Responsible to create <see href="https://docs.unity3d.com/ScriptReference/MeshFilter.html">MeshFilter</see>
        /// and <see href="https://docs.unity3d.com/ScriptReference/MeshRenderer.html">MeshRenderer</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Component.html">Components</see>.
        /// </summary>
        public GOCompositeRenderer()
        {
            m_MeshFilter = m_GameObject.AddComponent<MeshFilter>();
            m_MeshRenderer = m_GameObject.AddComponent<MeshRenderer>();
        }

        private int m_Layer;
        private Mesh m_Mesh;
        private UGMaterial[] m_Materials;
        private Action<UGMaterial>[] m_MaterialListeners;
        private readonly MeshFilter m_MeshFilter;
        private readonly MeshRenderer m_MeshRenderer;
        private List<SecondaryRenderer> m_SecondaryRenderers;

        /// <inheritdoc cref="GOBaseRenderer.Mesh"/>
        public override Mesh Mesh
        {
            set
            {
                Assert.IsTrue(value == null || value.subMeshCount == 1);
                m_Mesh = value;
                m_MeshFilter.sharedMesh = value;
                if (m_SecondaryRenderers != null)
                {
                    foreach (SecondaryRenderer renderer in m_SecondaryRenderers)
                    {
                        renderer.Mesh = value;
                    }
                }
            }
        }

        /// <inheritdoc cref="GOBaseRenderer.Materials"/>
        public override UGMaterial[] Materials
        {
            set
            {
                Assert.IsTrue(value == null || value.Length == 1);

                if (m_Materials != null)
                {
                    Assert.IsNotNull(m_MaterialListeners);
                    Assert.AreEqual(m_Materials.Length, m_MaterialListeners.Length);
                    for (int i = 0; i < m_Materials.Length; i++)
                        m_Materials[i].RemoveListener(m_MaterialListeners[i]);
                    m_Materials = null;
                    m_MaterialListeners = null;
                }

                if (value != null)
                {
                    Action<UGMaterial> listener = ugMaterial =>
                    {
                        Assert.IsTrue(ugMaterial.UnityMaterials.Count >= 1);

                        AdjustSecondaryRendererCount(ugMaterial.UnityMaterials.Count - 1);

                        m_MeshRenderer.sharedMaterial = ugMaterial.UnityMaterials[0];

                        if (m_SecondaryRenderers != null)
                        {
                            for (int i = 0; i < m_SecondaryRenderers.Count; i++)
                                m_SecondaryRenderers[i].Material = ugMaterial.UnityMaterials[i + 1];
                        }
                    };
                    value[0].AddListener(listener);
                    listener.Invoke(value[0]);
                }
            }
        }

        /// <inheritdoc cref="GOBaseRenderer.UnityLayer"/>
        public override int UnityLayer
        {
            set
            {
                m_Layer = value;
                m_GameObject.layer = value;
                if (m_SecondaryRenderers != null)
                {
                    foreach (SecondaryRenderer renderer in m_SecondaryRenderers)
                    {
                        renderer.UnityLayer = value;
                    }
                }
            }
        }

        private void AdjustSecondaryRendererCount(int count)
        {
            if (count < 0)
                count = 0;

            if (m_SecondaryRenderers == null && count > 1)
                m_SecondaryRenderers = new List<SecondaryRenderer>();

            while (m_SecondaryRenderers != null && m_SecondaryRenderers.Count < count)
            {
                SecondaryRenderer secondaryRenderer = new SecondaryRenderer();
                secondaryRenderer.Mesh = m_Mesh;
                secondaryRenderer.UnityLayer = m_Layer;
                m_SecondaryRenderers.Add(secondaryRenderer);
            }

            int secondaryCount;
            while (m_SecondaryRenderers != null && (secondaryCount = m_SecondaryRenderers.Count) > count)
            {
                m_SecondaryRenderers[secondaryCount - 1].Dispose();
                m_SecondaryRenderers.RemoveAt(secondaryCount - 1);
            }
        }
    }
}
