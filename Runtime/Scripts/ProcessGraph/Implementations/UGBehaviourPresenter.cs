using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Unity.Geospatial.Streaming
{

    public class UGBehaviourPresenter : UGPresenter
    {
        public UGBehaviourPresenter(Transform rootTransform, int unityLayer, List<UGDataSourceID> dataSources)
        {
            m_UnityLayer = unityLayer;

            m_ApplicableDataSources = dataSources;

            m_AvailableGameObjects = new GameObject("Available Game Objects").transform;
            m_AvailableGameObjects.parent = rootTransform;
            m_AvailableGameObjects.localPosition = Vector3.zero;
            m_AvailableGameObjects.localRotation = Quaternion.identity;
            m_AvailableGameObjects.localScale = Vector3.one;

            m_InUseGameObjects = new GameObject("In Use Game Objects").transform;
            m_InUseGameObjects.parent = rootTransform;
            m_InUseGameObjects.localPosition = Vector3.zero;
            m_InUseGameObjects.localRotation = Quaternion.identity;
            m_InUseGameObjects.localScale = Vector3.one;

            m_EmptyNodeRendererPool = new UGObjectPool<GOBaseRenderer>(
                        AllocateRenderer_Empty,
                        DisposeRenderer,
                        EnableRenderer,
                        DisableRenderer);

            m_CompositeRendererPool = new UGObjectPool<GOBaseRenderer>(
                        AllocateRenderer_Composite,
                        DisposeRenderer,
                        EnableRenderer,
                        DisableRenderer);

            m_GOMeshRendererPool = new UGObjectPool<GOBaseRenderer>(
                        AllocateRenderer,
                        DisposeRenderer,
                        EnableRenderer,
                        DisableRenderer);
        }
        private readonly Transform m_AvailableGameObjects;
        private readonly Transform m_InUseGameObjects;

        private readonly UGObjectPool<GOBaseRenderer> m_EmptyNodeRendererPool;
        private readonly UGObjectPool<GOBaseRenderer> m_CompositeRendererPool;
        private readonly UGObjectPool<GOBaseRenderer> m_GOMeshRendererPool;

        private readonly Queue<GOBaseRenderer> m_WaitForHPTransformRemoval = new Queue<GOBaseRenderer>();

        private readonly Dictionary<InstanceID, GOBaseRenderer> m_RenderObjects = new Dictionary<InstanceID, GOBaseRenderer>();

        private readonly int m_UnityLayer;

        private readonly List<UGDataSourceID> m_ApplicableDataSources;
        private readonly Queue<GOBaseRenderer> m_DisposeQueue = new Queue<GOBaseRenderer>();

        /// <summary>
        /// <see langword="Action"/> to be executed when ever a new <see cref="UniversalDecoder.NodeContent"/>
        /// get loaded in memory.
        /// </summary>
        /// <returns>
        ///     <typeparam>New instance id being created.
        ///         <name>instance</name>
        ///     </typeparam>
        ///     <typeparam>Game object linked with instance.
        ///         <name>gameObject</name>
        ///     </typeparam>
        /// </returns>
        public event Action<InstanceID, GameObject> OnAllocateInstance;

        /// <summary>
        /// <see langword="Action"/> to be executed when ever a <see cref="UniversalDecoder.NodeContent"/>
        /// loaded in memory get disposed of.
        /// </summary>
        /// <returns>
        ///     <typeparam>New instance id being disposed.
        ///         <name>instance</name>
        ///     </typeparam>
        ///     <typeparam>Game object linked with instance.
        ///         <name>gameObject</name>
        ///     </typeparam>
        /// </returns>
        public event Action<InstanceID, GameObject> OnDisposeInstance;
        
        /// <summary>
        /// <see langword="Action"/> to be executed when ever a <see cref="UniversalDecoder.NodeContent"/>
        /// visibility state changes.
        /// </summary>
        /// <returns>
        ///     <typeparam>New instance id being updated.
        ///         <name>instance</name>
        ///     </typeparam>
        ///     <typeparam>Game object linked with instance.
        ///         <name>gameObject</name>
        ///     </typeparam>
        ///     <typeparam>
        ///         <see langword="true"/> if the instance is visible and loaded;
        ///         <see langword="false"/> otherwise.
        ///         <name>isVisible</name>
        ///     </typeparam>
        /// </returns>
        public event Action<InstanceID, GameObject, bool> OnUpdateInstanceVisibility;

        public override void Dispose()
        {
            foreach(var kvp in m_RenderObjects)
            {
                ReturnChildrenThenSelfToPool(kvp.Value);
            }

            m_CompositeRendererPool.Dispose();
            m_GOMeshRendererPool.Dispose();
            m_EmptyNodeRendererPool.Dispose();

            DestroyGameObject(m_AvailableGameObjects.gameObject);
            DestroyGameObject(m_InUseGameObjects.gameObject);
        }


        protected override void CmdAllocate(InstanceID id, InstanceData instance)
        {
            if (!m_ApplicableDataSources.Contains(instance.Source))
                return;

            GOBaseRenderer renderer = CreateRenderer(instance);

            renderer.Parent = m_InUseGameObjects;
            renderer.EnableHighPrecision = true;
            renderer.Transform = instance.Transform;

            m_RenderObjects.Add(id, renderer);

            OnAllocateInstance?.Invoke(id, renderer.GameObject);
        }

        //
        //  TODO - Remove recursion?
        //
        private GOBaseRenderer CreateRenderer(InstanceData instance)
        {
            bool isComposite = IsComposite(instance.Materials);

            GOBaseRenderer renderer;
            if (!instance.IsRenderable)
                renderer = m_EmptyNodeRendererPool.GetObject();
            else if (isComposite)
                renderer = m_CompositeRendererPool.GetObject();
            else
                renderer = m_GOMeshRendererPool.GetObject();

            Assert.IsTrue(renderer.Children == null || renderer.Children.Count == 0);

            renderer.Name = instance.Name;
            renderer.Metadata = instance.Metadata;
            renderer.UnityLayer = m_UnityLayer;
            

            if (instance.IsRenderable)
            {
                renderer.Mesh = instance.Mesh;
                renderer.Materials = instance.Materials;
            }
            if (instance.Children != null)
            {
                foreach (InstanceData child in instance.Children)
                {
                    GOBaseRenderer childRenderer = CreateRenderer(child);
                    childRenderer.Enabled = true;
                    renderer.AddChild(childRenderer);
                    childRenderer.Transform = child.Transform;
                }
            }

            return renderer;
        }

        private static bool IsComposite(UGMaterial[] materials)
        {
            if (materials == null)
                return false;

            return materials.Length == 1 && materials[0].IsComposite;
        }

        protected override void CmdDispose(InstanceID id)
        {
            if (m_RenderObjects.TryGetValue(id, out GOBaseRenderer renderer))
            {
                if (renderer.Enabled)
                    Debug.LogWarning("Presenter is disposing of visible render instance, this should be avoided");

                m_RenderObjects.Remove(id);

                OnDisposeInstance?.Invoke(id, renderer.GameObject);
                ReturnChildrenThenSelfToPool(renderer);
            }
        }

        //
        // TODO - Remove recursion?
        //
        private void ReturnChildrenThenSelfToPool(GOBaseRenderer renderer)
        {
            Assert.AreEqual(0, m_DisposeQueue.Count);

            m_DisposeQueue.Enqueue(renderer);

            while (m_DisposeQueue.Count > 0)
            {
                renderer = m_DisposeQueue.Dequeue();

                EnqueueChildren(renderer, m_DisposeQueue);

                renderer.ClearChildren();

                ReleaseRenderer(renderer);
            }
        }

        private void ReleaseRenderer(GOBaseRenderer renderer)
        {
            Assert.IsTrue(renderer.Children == null || renderer.Children.Count == 0);
            renderer.Enabled = false;
            renderer.EnableHighPrecision = false;
            renderer.Name = "Available";
            renderer.Parent = m_AvailableGameObjects;
            renderer.Mesh = null;
            renderer.Materials = null;
            m_WaitForHPTransformRemoval.Enqueue(renderer);
        }


        private static void EnqueueChildren(GOBaseRenderer renderer, Queue<GOBaseRenderer> queue)
        {
            if (renderer.Children == null)
                return;

            foreach (IUGRenderer child in renderer.Children)
                queue.Enqueue((GOBaseRenderer)child);
        }

        protected override void CmdUpdateVisibility(InstanceID id, bool isVisible)
        {
            if (m_RenderObjects.TryGetValue(id, out GOBaseRenderer renderer) 
                && renderer.Enabled != isVisible)
            {
                renderer.Enabled = isVisible;
                OnUpdateInstanceVisibility?.Invoke(id, renderer.GameObject, isVisible);
            }
        }


        private GOBaseRenderer AllocateRenderer()
        {
            GOBaseRenderer result = new GOMeshRenderer();
            result.Name = "Available Child Renderer";
            result.Parent = m_AvailableGameObjects;
            return result;
        }

        private GOBaseRenderer AllocateRenderer_Composite()
        {
            GOBaseRenderer result = new GOCompositeRenderer();
            result.Name = "Available Renderer";
            result.Parent = m_AvailableGameObjects;
            return result;
        }

        private GOBaseRenderer AllocateRenderer_Empty()
        {
            GOBaseRenderer result = new GONodeRenderer();
            result.Name = "Available Renderer";
            result.Parent = m_AvailableGameObjects;
            return result;
        }

        private void DisposeRenderer(GOBaseRenderer renderer)
        {
            renderer.Dispose();
        }

        private void EnableRenderer(GOBaseRenderer renderer)
        {
            Assert.IsTrue(renderer.Children == null || renderer.Children.Count == 0);
        }

        private void DisableRenderer(GOBaseRenderer renderer)
        {
            //
            //  Intentionally left blank
            //
        }

        private void DestroyGameObject(GameObject gameObject)
        {
            if (Application.isPlaying)
                Object.Destroy(gameObject);
            else
                Object.DestroyImmediate(gameObject);
        }

        public override void MainThreadUpKeep()
        {
            while (m_WaitForHPTransformRemoval.Count > 0)
            {
                GOBaseRenderer renderer = m_WaitForHPTransformRemoval.Dequeue();

                if (renderer.GetType() == typeof(GONodeRenderer))
                    m_EmptyNodeRendererPool.ReleaseObject(renderer);
                else if (renderer.GetType() == typeof(GOCompositeRenderer))
                    m_CompositeRendererPool.ReleaseObject(renderer);
                else
                    m_GOMeshRendererPool.ReleaseObject(renderer);

            }
        }
    }

}
