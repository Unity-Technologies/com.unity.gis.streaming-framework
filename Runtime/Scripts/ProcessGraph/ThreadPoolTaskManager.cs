using System;
using System.Threading;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The thread pool task manager schedules all tasks onto the C#
    /// thread pool in order to pull it away from the main thread. It is
    /// guaranteed that the task will never be scheduled on the main thread.
    /// </summary>
    internal class ThreadPoolTaskManager : IExecutableTaskManager
    {
        /// <summary>
        /// Lock used to prevent concurent access to task counter
        /// </summary>
        private readonly object m_Lock = new object();

        /// <inheritdoc cref="IExecutableTaskManager.MainThreadTasks"/>
        public int MainThreadTasks { get { return 0; } }

        /// <inheritdoc cref="IExecutableTaskManager.ThreadPoolTasks"/>
        public int ThreadPoolTasks { get; private set; }

        /// <inheritdoc cref="ITaskManager.ScheduleTask(Action)"/>
        public void ScheduleTask(Action task)
        {
            lock (m_Lock)
            {
                ThreadPoolTasks++;
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                task.Invoke();
                lock (m_Lock)
                {
                    ThreadPoolTasks--;
                }
            });
        }

        /// <inheritdoc cref="IExecutableTaskManager.ExecuteMainThreadTask"/>
        public void ExecuteMainThreadTask()
        {
            //
            //  Intentionally left blank
            //
        }
    }
}
