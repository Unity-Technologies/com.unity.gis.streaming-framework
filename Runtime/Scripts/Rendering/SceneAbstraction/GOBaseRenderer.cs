using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Base class responsible for converting <see cref="InstanceID"/> into a <see cref="GameObject"/>.
    /// </summary>
    public abstract class GOBaseRenderer :
        IUGRenderer,
        IUGObject
    {
        /// <summary>
        /// Default constructor.
        /// Will create an inactive <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see> and
        /// attach a <see cref="UGMetadataBehaviour"/> <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see>
        /// to it.
        /// </summary>
        protected GOBaseRenderer()
        {
            m_GameObject = new GameObject();

            m_Transform = m_GameObject.transform;
            m_HPTransform = null;
            m_MetadataBehaviour = m_GameObject.AddComponent<UGMetadataBehaviour>();

            m_GameObject.SetActive(false);
            m_Enabled = false;
            m_GameObject.hideFlags = HideFlags.DontSave;
        }

        /// <inheritdoc cref="IUGRenderer.Enabled"/>
        private bool m_Enabled;
        
        /// <summary>
        /// Cached <see cref="UGMetadataBehaviour"/> <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see>
        /// of the <see cref="GameObject"/>.
        /// </summary>
        private readonly UGMetadataBehaviour m_MetadataBehaviour;
        
        /// <summary>
        /// Cached HPTransform <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see> of the <see cref="GameObject"/>.
        /// </summary>
        private HPTransform m_HPTransform;
        
        /// <inheritdoc cref="IUGRenderer.Children"/>
        private List<IUGRenderer> m_Children;

        /// <summary>
        /// Get the <see href="https://docs.unity3d.com/ScriptReference/GameObject">GameObject</see> that holds the
        /// <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see> and required
        /// <see href="https://docs.unity3d.com/ScriptReference/Component.html">Components</see> when in play mode.
        /// </summary>
        protected readonly GameObject m_GameObject;
        
        /// <summary>
        /// Cached <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see> of the <see cref="GameObject"/>.
        /// </summary>
        protected readonly Transform m_Transform;

        /// <summary>
        /// Get the <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see> <see cref="Children"/>
        /// are attached to.
        /// </summary>
        public GameObject GameObject
        {
            get { return m_GameObject; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Assert.IsTrue(m_Children == null || m_Children.Count == 0);

            if (m_GameObject != null)
                Object.Destroy(m_GameObject);
        }

        /// <inheritdoc cref="IUGRenderer.Enabled"/>
        public bool Enabled
        {
            get { return m_Enabled; }
            set
            {
                m_Enabled = value;
                m_GameObject.SetActive(m_Enabled);
            }
        }

        /// <inheritdoc cref="IUGObject.Disposed"/>
        bool IUGObject.Disposed { get; set; }

        /// <inheritdoc cref="IUGObject.Index"/>
        int IUGObject.Index { get; set; }

        /// <inheritdoc cref="IUGRenderer.Name"/>
        public string Name
        {
            set { m_GameObject.name = value; }
        }

        /// <inheritdoc cref="IUGRenderer.Parent"/>
        public Transform Parent
        {
            set
            {
                if (m_HPTransform != null)
                    m_HPTransform.Parent = value;
                else
                    m_Transform.parent = value;
            }
        }
        
        /// <inheritdoc cref="IUGRenderer.Transform"/>
        public double4x4 Transform
        {
            set
            {
                value.GetTRS(out double3 position, out quaternion rotation, out float3 scale);
                if (m_HPTransform == null)
                {
                    m_Transform.localPosition = (float3)position;
                    m_Transform.localRotation = rotation;
                    m_Transform.localScale = scale;
                }
                else
                {
                    m_HPTransform.LocalPosition = position;
                    m_HPTransform.LocalRotation = rotation;
                    m_HPTransform.LocalScale = scale;
                }
            }
        }
        
        /// <inheritdoc cref="IUGRenderer.Metadata"/>
        public UGMetadata Metadata
        {
            set
            {
                m_MetadataBehaviour.Metadata = value;
            }
        }

        /// <inheritdoc cref="IUGRenderer.Children"/>
        public List<IUGRenderer> Children
        {
            get { return m_Children; }
        }

        /// <inheritdoc cref="IUGRenderer.EnableHighPrecision"/>
        public bool EnableHighPrecision
        {
            set
            {
                if (value && m_HPTransform == null)
                {
                    m_HPTransform = m_GameObject.AddComponent<HPTransform>();
                }
                else if (!value && m_HPTransform != null)
                {
                    if (Application.isPlaying)
                        Object.Destroy(m_HPTransform);
                    else
                        Object.DestroyImmediate(m_HPTransform);

                    m_HPTransform = null;
                }
            }
        }

        /// <inheritdoc cref="IUGRenderer.UnityLayer"/>
        public abstract int UnityLayer { set; }
        
        /// <inheritdoc cref="IUGRenderer.Mesh"/>
        public abstract Mesh Mesh { set; }
        
        /// <inheritdoc cref="IUGRenderer.Materials"/>
        public abstract UGMaterial[] Materials { set; }

        /// <inheritdoc cref="IUGRenderer.AddChild(IUGRenderer)"/>
        public void AddChild(IUGRenderer child)
        {
            if (m_Children == null)
                m_Children = new List<IUGRenderer>();

            m_Children.Add(child);
            child.Parent = m_Transform;
        }

        /// <inheritdoc cref="IUGRenderer.RemoveChild(IUGRenderer)"/>
        public void RemoveChild(IUGRenderer child)
        {
            m_Children.Remove(child);
            child.Parent = null;
        }

        /// <inheritdoc cref="IUGRenderer.ClearChildren()"/>
        public void ClearChildren()
        {
            if (m_Children == null)
                return;

            foreach (IUGRenderer child in m_Children)
            {
                child.Parent = null;
            }

            m_Children.Clear();
        }
    }
}
