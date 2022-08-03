using System;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Main interface defining how to execute the
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Tasks</see>.
    /// This allow to implement either a custom task manager or decide whether all the
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Tasks</see> should be
    /// executed on the main thread or be multi-threaded.
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// Schedule a task to run outside of the main thread. Some implementation may
        /// prefer to run the task on the main thread anyways in order to support single
        /// thread platforms or in order to optimise performance.
        /// </summary>
        /// <param name="task">The action to be scheduled on by the task manager</param>
        void ScheduleTask(Action task);
    }
}
