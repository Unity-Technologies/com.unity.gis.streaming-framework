
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// This interface extension allows both single thread and multi-thread implementations of
    /// the task manager to be called in the same way from within the <see cref="UGSystem"/>.
    /// </summary>
    internal interface IExecutableTaskManager : ITaskManager
    {
        /// <summary>
        /// The number of tasks scheduled to run on the main thread.
        /// </summary>
        public int MainThreadTasks { get; }

        /// <summary>
        /// The number of tasks scheduled to run in the thread pool
        /// </summary>
        public int ThreadPoolTasks { get; }

        /// <summary>
        /// A method which must be called regularly if tasks are scheduled to run on the
        /// main thread.
        /// </summary>
        public void ExecuteMainThreadTask();
    }
}
