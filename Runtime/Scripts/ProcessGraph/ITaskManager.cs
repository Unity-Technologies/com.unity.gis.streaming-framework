using System;

namespace Unity.Geospatial.Streaming
{
    public interface ITaskManager
    {
        void ScheduleTask(Action task);
    }
}
