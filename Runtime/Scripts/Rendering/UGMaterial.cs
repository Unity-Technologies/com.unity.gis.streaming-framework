using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Base class used by the <see cref="UGMaterialFactory"/> allowing to create
    /// Unity <see href="https://docs.unity3d.com/ScriptReference/Material.html">Materials</see> to be assigned to instances
    /// created by the <see cref="IUGRenderer">renderer</see>.
    /// </summary>
    public abstract class UGMaterial : IDisposable
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="isComposite"></param>
        protected UGMaterial(bool isComposite)
        {
            IsComposite = isComposite;
        }

        private bool m_Disposed;
        private Action<UGMaterial> m_OnChangeListeners;

        /// <summary>
        /// <see langword="true"/> if the material is a composition of multiple overlaying <see href="https://docs.unity3d.com/ScriptReference/Material.html">materials</see>;
        /// <see langword="false"/> otherwise.
        /// </summary>
        public bool IsComposite { get; }

        /// <summary>
        /// Created <see href="https://docs.unity3d.com/ScriptReference/Material.html">Materials</see> after the
        /// renderer instantiation was executed via the corresponding <see cref="IUGRenderer"/>.
        /// </summary>
        public abstract List<Material> UnityMaterials { get; }

        /// <summary>
        /// Called whenever <see cref="UGCommandBuffer.IListener.AddMaterialProperty(MaterialID, MaterialProperty)"/> request
        /// to create a new <see cref="MaterialProperty"/> for this material.
        /// </summary>
        /// <param name="materialProperty">Property to add.</param>
        protected abstract void OnAddMaterialProperty(MaterialProperty materialProperty);

        /// <summary>
        /// Called whenever <see cref="UGCommandBuffer.IListener.AddMaterialProperty(MaterialID, MaterialProperty)"/> request
        /// to remove a <see cref="MaterialProperty"/> from this material.
        /// </summary>
        /// <param name="materialProperty">Property to remove.</param>
        protected abstract void OnRemoveMaterialProperty(MaterialProperty materialProperty);

        /// <summary>
        /// Called when <see cref="Dispose"/> is called allowing to release unmanaged resources.
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            OnDispose();
            m_Disposed = true;
        }

        /// <summary>
        /// Destructor called by the garbage collector.
        /// </summary>
        ~UGMaterial()
        {
            Assert.IsTrue(m_Disposed);
        }

        /// <summary>
        /// Register an <see href="https://docs.microsoft.com/en-us/dotnet/api/system.action-1">Action</see> to be called
        /// after <see cref="AddMaterialProperty"/> execution.
        /// </summary>
        /// <param name="listener">Execute this <see href="https://docs.microsoft.com/en-us/dotnet/api/system.action-1">Action</see>
        /// with the affected <see cref="UGMaterial"/> as the given argument.</param>
        public void AddListener(Action<UGMaterial> listener)
        {
            m_OnChangeListeners += listener;
        }

        /// <summary>
        /// Unregister an <see href="https://docs.microsoft.com/en-us/dotnet/api/system.action-1">Action</see> that was called
        /// after <see cref="AddMaterialProperty"/> execution.
        /// </summary>
        /// <param name="listener"><see href="https://docs.microsoft.com/en-us/dotnet/api/system.action-1">Action</see> to be unregistered.</param>
        public void RemoveListener(Action<UGMaterial> listener)
        {
            m_OnChangeListeners -= listener;
        }

        private void UpdateListeners()
        {
            if (m_OnChangeListeners != null) m_OnChangeListeners.Invoke(this);
        }

        /// <summary>
        /// Apply the given <paramref name="materialProperty">property</paramref> to the material.
        /// </summary>
        /// <param name="materialProperty">Property to add to the material.</param>
        public void AddMaterialProperty(MaterialProperty materialProperty)
        {
            OnAddMaterialProperty(materialProperty);
            UpdateListeners();
        }

        /// <summary>
        /// Remove a previously <see cref="AddMaterialProperty">added</see> <paramref name="materialProperty">property</paramref> from this material.
        /// </summary>
        /// <param name="materialProperty">Property to remove from the material.</param>
        public void RemoveMaterialProperty(MaterialProperty materialProperty)
        {
            OnRemoveMaterialProperty(materialProperty);
            UpdateListeners();
        }
    }
}
