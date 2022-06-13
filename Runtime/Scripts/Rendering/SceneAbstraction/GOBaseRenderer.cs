using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    public abstract class GOBaseRenderer :
        IUGRenderer,
        IUGObject
    {
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

        private bool m_Enabled;
        private readonly UGMetadataBehaviour m_MetadataBehaviour;
        private HPTransform m_HPTransform;
        private List<IUGRenderer> m_Children;

        protected readonly GameObject m_GameObject;
        protected readonly Transform m_Transform;

        public GameObject GameObject
        {
            get { return m_GameObject; }
        }

        public void Dispose()
        {
            Assert.IsTrue(m_Children == null || m_Children.Count == 0);

            if (m_GameObject != null)
                Object.Destroy(m_GameObject);
        }

        public bool Enabled
        {
            get => m_Enabled;
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

        public string Name
        {
            set => m_GameObject.name = value;
        }
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
        public UGMetadata Metadata
        {
            set
            {
                m_MetadataBehaviour.Metadata = value;
            }
        }

        public List<IUGRenderer> Children
        {
            get { return m_Children; }
        }

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

        public abstract int UnityLayer { set; }
        public abstract Mesh Mesh { set; }
        public abstract UGMaterial[] Materials { set; }

        public void AddChild(IUGRenderer child)
        {
            if (m_Children == null)
                m_Children = new List<IUGRenderer>();

            m_Children.Add(child);
            child.Parent = m_Transform;
        }

        public void RemoveChild(IUGRenderer child)
        {
            m_Children.Remove(child);
            child.Parent = null;
        }

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
