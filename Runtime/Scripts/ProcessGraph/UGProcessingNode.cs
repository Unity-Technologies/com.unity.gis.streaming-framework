using System;

namespace Unity.Geospatial.Streaming
{
    //
    //  TODO - Enforce only passing data through main thread
    //
    public abstract class UGProcessingNode : IDisposable
    {
        private class TaskManagerWrapper : ITaskManager
        {
            private ITaskManager m_Implementation;

            public void ScheduleTask(Action task)
            {
                m_Implementation?.ScheduleTask(task);
            }

            public void SetTaskManager(ITaskManager implementation)
            {
                m_Implementation = implementation;
            }
        }


        public enum DataAvailability
        {
            WaitingUpstream,
            Processing,            
            Idle,
        }

        public abstract class NodeInput<T> : NodeInput
        {
            public NodeOutput<T> ConnectedOutput { get; set; }

            public abstract bool IsReadyForData { get; }

            public abstract void ProcessData(ref T data);

            public override DataAvailability UpstreamDataAvailability
            {
                get
                {
                    if (ConnectedOutput == null)
                        return DataAvailability.Idle;

                    return ConnectedOutput.UpstreamDataAvailability;
                }
            }
        }

        public abstract class NodeInput
        {
            public abstract DataAvailability UpstreamDataAvailability { get; }
        }

        public class NodeOutput<T> : NodeOutput
        {
            public NodeOutput(UGProcessingNode node)
            {
                m_Node = node;
            }

            private readonly UGProcessingNode m_Node;
            public NodeInput<T> ConnectedInput { get; set; }

            public bool IsReadyForData
            {
                get
                {
                    if (ConnectedInput == null)
                        return true;
                    
                    return ConnectedInput.IsReadyForData;
                }
            }

            public void ProcessData(ref T data)
            {
                if (ConnectedInput != null)
                    ConnectedInput.ProcessData(ref data);
            }

            public override DataAvailability UpstreamDataAvailability => m_Node.GetDataAvailabilityStatus(this);
            
            
        }

        public abstract class NodeOutput
        { 
            public abstract DataAvailability UpstreamDataAvailability { get; }
        }


        protected UGProcessingNode()
        {
            TaskManager = m_TaskManagerWrapper = new TaskManagerWrapper();
        }


        protected abstract bool IsProcessing { get; }

        /// <summary>
        /// <see langword="true"/> if all nodes part of this instance are with the status idle;
        /// <see langword="false"/> otherwise.
        /// </summary>
        public bool IsIdle
        {
            get { return GetDataAvailabilityStatus(null) == DataAvailability.Idle; }
        }

        public DataAvailability DataAvailabilityStatus
        {
            get { return GetDataAvailabilityStatus(null); }
        }


        protected virtual DataAvailability GetDataAvailabilityStatus(NodeOutput output)
        {
            if (IsProcessing)
                return DataAvailability.Processing;

            if (InputConnections == null)
                return DataAvailability.Idle;

            foreach (var input in InputConnections)
            {
                if (input.UpstreamDataAvailability != DataAvailability.Idle)
                    return DataAvailability.WaitingUpstream;
            }

            return DataAvailability.Idle;
        }


        public abstract void Dispose();

        public abstract bool ScheduleMainThread { get; }

        public abstract void MainThreadUpKeep();
        public abstract void MainThreadProcess();

        private readonly TaskManagerWrapper m_TaskManagerWrapper;
        protected readonly ITaskManager TaskManager;
        protected NodeInput[] InputConnections;
        protected NodeOutput[] OutputConnections;

        public void SetTaskManager(ITaskManager taskManager)
        {
            m_TaskManagerWrapper.SetTaskManager(taskManager);
        }

        public static void Connect<T>(NodeOutput<T> leftNode, NodeInput<T> rightNode)
        {
            leftNode.ConnectedInput = rightNode;
            rightNode.ConnectedOutput = leftNode;
        }

    }
}
