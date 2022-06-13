using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    public abstract class UGMaterial : IDisposable
    {
        protected UGMaterial(bool isComposite)
        {
            IsComposite = isComposite;
        }

        private bool m_Disposed;
        private Action<UGMaterial> m_OnChangeListeners;

        public bool IsComposite { get; }

        public abstract List<Material> UnityMaterials { get; }

        protected abstract void OnAddMaterialProperty(MaterialProperty materialProperty);

        protected abstract void OnRemoveMaterialProperty(MaterialProperty materialProperty);

        protected abstract void OnDispose();

        public virtual void Dispose()
        {
            OnDispose();
            m_Disposed = true;
        }

        ~UGMaterial()
        {
            Assert.IsTrue(m_Disposed);
        }

        public void AddListener(Action<UGMaterial> listener)
        {
            m_OnChangeListeners += listener;
        }

        public void RemoveListener(Action<UGMaterial> listener)
        {
            m_OnChangeListeners -= listener;
        }

        private void UpdateListeners()
        {
            if (m_OnChangeListeners != null) m_OnChangeListeners.Invoke(this);
        }


        public void AddMaterialProperty(MaterialProperty materialProperty)
        {
            OnAddMaterialProperty(materialProperty);
            UpdateListeners();
        }

        public void RemoveMaterialProperty(MaterialProperty materialProperty)
        {
            OnRemoveMaterialProperty(materialProperty);
            UpdateListeners();
        }
    }
}
