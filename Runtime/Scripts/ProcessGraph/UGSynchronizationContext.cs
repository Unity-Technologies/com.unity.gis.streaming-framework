
using System.Collections.Concurrent;
using System.Threading;

using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Determines how async / await calls are going to be resolved.
    /// The <see cref="UGSynchronizationContext"/> is used within the <see cref="UGSystem"/> and replaces the
    /// <see cref="UnitySynchronizationContext"/> within its execution flow.
    /// In contrary to the <see cref="UnitySynchronizationContext"/>, the <see cref="UGSynchronizationContext"/> may
    /// continue asynchronous operation within the same frame, if the <see cref="UGSystem">UGSystem's</see> CPU time
    /// limit has not yet been reached.
    /// </summary>
    internal sealed class UGSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<WorkRequest> m_Queue;
        private SynchronizationContext m_PreviousContext;

        public UGSynchronizationContext()
        {
            m_Queue = new BlockingCollection<WorkRequest>();
        }

        private UGSynchronizationContext(BlockingCollection<WorkRequest> queue)
        {
            m_Queue = queue;
        }

        /// <summary>
        /// When overridden in a derived class, dispatches a synchronous message to a synchronization context.
        /// </summary>
        /// <param name="callback">The delegate to call.</param>
        /// <param name="state">State of the process.</param>
        public override void Send(SendOrPostCallback callback, object state)
        {
            callback(state);
        }

        /// <summary>
        /// When overridden in a derived class, dispatches an asynchronous message to a synchronization context.
        /// </summary>
        /// <param name="callback">The delegate to call.</param>
        /// <param name="state">State of the process.</param>
        public override void Post(SendOrPostCallback callback, object state)
        {
            m_Queue.Add(new WorkRequest(callback, state));
        }

        public override SynchronizationContext CreateCopy()
        {
            return new UGSynchronizationContext(m_Queue);
        }

        public bool ScheduleMainThread
        {
            get { return m_Queue.Count > 0; }
        }

        // Exec will execute tasks off the task list
        public void MainThreadProcess()
        {
            Assert.AreNotEqual(0, m_Queue.Count);

            WorkRequest work = m_Queue.Take();
            work.Invoke();
        }

        public void SetContext()
        {
            Assert.IsNull(m_PreviousContext);
            m_PreviousContext = Current;
            SetSynchronizationContext(this);
        }

        public void ResetContext()
        {
            Assert.IsNotNull(m_PreviousContext);
            SetSynchronizationContext(m_PreviousContext);
            m_PreviousContext = null;
        }

        private struct WorkRequest
        {
            private readonly SendOrPostCallback m_DelegateCallback;
            private readonly object m_DelegateState;
            private readonly ManualResetEvent m_WaitHandle;

            public WorkRequest(SendOrPostCallback callback, object state, ManualResetEvent waitHandle = null)
            {
                m_DelegateCallback = callback;
                m_DelegateState = state;
                m_WaitHandle = waitHandle;
            }

            public void Invoke()
            {
                try
                {
                    m_DelegateCallback(m_DelegateState);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }

                if (m_WaitHandle != null)
                    m_WaitHandle.Set();
            }
        }
    }
}
