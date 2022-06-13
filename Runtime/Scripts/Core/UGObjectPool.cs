using System;
using System.Collections.Generic;

using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Allow to create new objects by limiting the garbage collection. Each time an object is disposed,
    /// it's only flagged has disabled allowing to reuse the object next time a new instance is requested.
    /// </summary>
    /// <typeparam name="T">Type of object created by <see cref="GetObject"/>.</typeparam>
    public class UGObjectPool<T> : 
        IDisposable
        where T: IUGObject
    {
        /// <summary>
        /// List of object currently instantiated.
        /// </summary>
        private readonly List<T> m_Pool = new List<T>();

        /// <summary>
        /// Stack of <see cref="IUGObject.Index"/> that where <see cref="ReleaseObject">Released</see>
        /// allowing reusing the object where <see cref="GetObject"/> is called instead of creating a new instance
        /// of <typeparamref name="T"/>.
        /// </summary>
        private readonly Stack<int> m_AvailableIndices = new Stack<int>();

        /// <summary>
        /// <see langword="true"/> if the pool instance was disposed and cannot generate new <typeparamref name="T"/> instances;
        /// <see langword="false"/> otherwise.
        /// </summary>
        private bool m_Disposed;
        
        /// <summary>
        /// Function used to create a new instance of <typeparamref name="T"/>.
        /// </summary>
        private readonly Func<T> m_Allocate;
        
        /// <summary>
        /// Function called when <see cref="ReleaseObject"/> is called for a <typeparamref name="T"/> instance.
        /// </summary>
        private readonly Action<T> m_Clear;
        
        /// <summary>
        /// Function called when this <see cref="UGObjectPool{T}"/> instance is disposed.
        /// </summary>
        private readonly Action<T> m_Dispose;
        
        /// <summary>
        /// Function called each time <see cref="GetObject"/> is called, even if it returns a recycled instance.
        /// </summary>
        private readonly Action<T> m_Set;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="allocate">Function used to create a new instance of <typeparamref name="T"/>.</param>
        /// <param name="dispose">Function called when this <see cref="UGObjectPool{T}"/> instance is disposed.</param>
        /// <param name="set">Function called each time <see cref="GetObject"/> is called, even if it returns a recycled instance.</param>
        /// <param name="clear">Function called when <see cref="ReleaseObject"/> is called for a <typeparamref name="T"/> instance.</param>
        public UGObjectPool(Func<T> allocate, Action<T> dispose, Action<T> set, Action<T> clear)
        {
            m_Allocate = allocate;
            m_Dispose = dispose;
            m_Set = set;
            m_Clear = clear;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// This will call the registered dispose method on each <typeparamref name="T"/> instances part of this pool.
        /// </summary>
        public void Dispose()
        {
            foreach (T obj in m_Pool)
            {
                obj.Disposed = true;
                m_Dispose(obj);
            }

            m_Disposed = true;
        }

        /// <summary>
        /// Destructor called by the garbage collector.
        /// </summary>
        ~UGObjectPool()
        {
            Assert.IsTrue(m_Disposed, "Object Pool has not been disposed. Behaviour may be undefined.");
        }

        /// <summary>
        /// Get the next available <typeparamref name="T"/> instance. If no more instances are available,
        /// a new instance will be created.
        /// </summary>
        /// <returns>A ready to use <typeparamref name="T"/> instance.</returns>
        public T GetObject()
        {
            Assert.IsFalse(m_Disposed);

            T result;

            if (m_AvailableIndices.Count != 0)
            {
                int index = m_AvailableIndices.Pop();
                result = m_Pool[index];
                result.Disposed = false;
            }
            else
            {
                result = m_Allocate();
                result.Index = m_Pool.Count;
                result.Disposed = false;
                m_Pool.Add(result);
            }

            m_Set(result);
            return result;
        }

        /// <summary>
        /// Mark a <typeparamref name="T"/> instance as no more used and make it available the next time
        /// <see cref="GetObject"/> is called.
        /// </summary>
        /// <param name="obj">The instance to tag as disposed.</param>
        public void ReleaseObject(T obj)
        {
            Assert.IsFalse(m_Disposed);
            Assert.IsFalse(obj.Disposed);

            obj.Disposed = true;
            m_AvailableIndices.Push(obj.Index);
            
            m_Clear(obj);
        }
    }
}
