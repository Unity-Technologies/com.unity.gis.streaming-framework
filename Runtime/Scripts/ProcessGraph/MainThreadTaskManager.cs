using System;
using System.Collections.Generic;


namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// An implementation of the task manager which solely runs on the main thread.
    /// </summary>
    internal class MainThreadTaskManager : IExecutableTaskManager
    {
        /// <summary>
        /// The queue of tasks to run on the main thread.
        /// </summary>
        private Queue<Action> m_Tasks = new Queue<Action>();


        /// <inheritdoc cref="IExecutableTaskManager.MainThreadTasks"/>
        public int MainThreadTasks { get { return m_Tasks.Count; } }

        /// <inheritdoc cref="IExecutableTaskManager.ThreadPoolTasks"/>
        public int ThreadPoolTasks { get { return 0; } }

        /// <inheritdoc cref="ITaskManager.ScheduleTask"/>
        public void ScheduleTask(Action task)
        {
            m_Tasks.Enqueue(task);
        }

        /// <inheritdoc cref="IExecutableTaskManager.ExecuteMainThreadTask"/>
        public void ExecuteMainThreadTask()
        {
            if (MainThreadTasks > 0)
                m_Tasks.Dequeue().Invoke();
        }
    }
}
