using System;

namespace Unity.Geospatial.Streaming
{
    //
    //  TODO - Enforce only passing data through main thread
    //
    /// <summary>
    /// Base class to execute a task by the <see cref="UGSystem"/> within a processing graph.
    /// </summary>
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

        /// <summary>
        /// State of the <see cref="UGProcessingNode"/>.
        /// </summary>
        public enum DataAvailability
        {
            /// <summary>
            /// The <see cref="UGProcessingNode"/> cannot be executed since at least one upstream <see cref="UGProcessingNode"/>
            /// haven't completed its execution.
            /// </summary>
            WaitingUpstream,
            
            /// <summary>
            /// The <see cref="UGProcessingNode"/> is currently executing.
            /// </summary>
            Processing,
            
            /// <summary>
            /// The <see cref="UGProcessingNode"/> does not need to be executed and is waiting to be called.
            /// </summary>
            Idle,
        }

        /// <summary>
        /// Generic input connector allowing to connect upstream values to an <see cref="NodeOutput{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of value this instances expect to receive when executing its parent <see cref="UGProcessingNode"/>.</typeparam>
        public abstract class NodeInput<T> : NodeInput
        {
            /// <summary>
            /// Define the dependency and the value getter of this instance.
            /// </summary>
            public NodeOutput<T> ConnectedOutput { get; set; }

            /// <summary>
            /// Get if the dependency was executed, meaning, ready to get its <see cref="ConnectedOutput"/> value.
            /// </summary>
            public abstract bool IsReadyForData { get; }

            /// <summary>
            /// Execute the <see cref="UGProcessingNode"/> owner of this input based on the given <paramref name="data"/> value.
            /// </summary>
            /// <param name="data">Execute the <see cref="UGProcessingNode"/> owner based on this given value.</param>
            public abstract void ProcessData(ref T data);

            /// <summary>
            /// Get if the <see cref="ConnectedOutput"/> is currently processing, preventing this instance to be processed.
            /// </summary>
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

        /// <summary>
        /// Base input connector allowing to connect upstream values to an <see cref="NodeOutput"/> as the source value.
        /// </summary>
        public abstract class NodeInput
        {
            /// <summary>
            /// Get if a dependency of this instance is currently processing, preventing this instance to be processed.
            /// </summary>
            public abstract DataAvailability UpstreamDataAvailability { get; }
        }

        /// <summary>
        /// Generic output connector allowing to connect downstream values to an <see cref="NodeInput{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of value this instances send when executing its parent <see cref="UGProcessingNode"/>.</typeparam>
        public class NodeOutput<T> : NodeOutput
        {
            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="node">Owner of this instance.</param>
            public NodeOutput(UGProcessingNode node)
            {
                m_Node = node;
            }

            /// <summary>
            /// Owner of the node connector.
            /// </summary>
            private readonly UGProcessingNode m_Node;
            
            /// <summary>
            /// The connected input using this output as upstream dependency.
            /// </summary>
            public NodeInput<T> ConnectedInput { get; set; }

            /// <summary>
            /// <see langword="true"/> if the <see cref="UGProcessingNode"/> parent can be executed;
            /// <see langword="false"/> if the <see cref="ConnectedInput"/> is not connected.
            /// </summary>
            public bool IsReadyForData
            {
                get
                {
                    if (ConnectedInput == null)
                        return true;
                    
                    return ConnectedInput.IsReadyForData;
                }
            }

            /// <summary>
            /// Execute the <see cref="UGProcessingNode"/> owner of this output based on the given <paramref name="data"/> value.
            /// </summary>
            /// <param name="data">Execute the <see cref="UGProcessingNode"/> owner based on this given value.</param>
            public void ProcessData(ref T data)
            {
                if (ConnectedInput != null)
                    ConnectedInput.ProcessData(ref data);
            }

            /// <summary>
            /// Get if this parent node currently processing and prevent downstream connections to be executed.
            /// </summary>
            public override DataAvailability UpstreamDataAvailability => m_Node.GetDataAvailabilityStatus(this);
        }

        /// <summary>
        /// Base output connector allowing to connect downstream values to an <see cref="NodeInput"/>.
        /// </summary>
        public abstract class NodeOutput
        { 
            /// <summary>
            /// Get if this parent node currently processing and prevent downstream connections to be executed.
            /// </summary>
            public abstract DataAvailability UpstreamDataAvailability { get; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected UGProcessingNode()
        {
            TaskManager = m_TaskManagerWrapper = new TaskManagerWrapper();
        }

        /// <summary>
        /// <see langword="true"/> if the node is currently being executed and cannot be called until completion;
        /// <see langword="false"/> otherwise.
        /// </summary>
        protected abstract bool IsProcessing { get; }

        /// <summary>
        /// Get if this instance has completed its execution.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if all nodes part of this instance are with the status idle;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool IsIdle
        {
            get { return GetDataAvailabilityStatus(null) == DataAvailability.Idle; }
        }
        
        /// <summary>
        /// Get if this node is currently processing and prevent downstream connections to be executed.
        /// </summary>
        /// <returns>The status of this instance execution.</returns>
        public DataAvailability DataAvailabilityStatus
        {
            get { return GetDataAvailabilityStatus(null); }
        }
        
        /// <summary>
        /// Get if this node has completed the execution for the given output value.
        /// </summary>
        /// <param name="output">Get the data availability for this output.</param>
        /// <returns>The status of this instance execution.</returns>
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// <see langword="true"/> if the process needs to be executed on the main thread;
        /// <see langword="false"/> otherwise.
        /// </summary>
        public abstract bool ScheduleMainThread { get; }

        /// <summary>
        /// At each new frame, this method will be executed before the main thread processes allowing pre-execution.
        /// </summary>
        public abstract void MainThreadUpKeep();
        
        /// <summary>
        /// At the end of each new frame, this method will be called as processes to be executed on the main thread.
        /// </summary>
        public abstract void MainThreadProcess();

        private readonly TaskManagerWrapper m_TaskManagerWrapper;
        
        /// <summary>
        /// Define how to execute the
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Tasks</see>.
        /// This allow to implement either a custom task manager or decide whether all the
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Tasks</see> should be
        /// executed on the main thread or be multi-threaded.
        /// </summary>
        protected readonly ITaskManager TaskManager;
        
        /// <summary>
        /// Array of required inputs to be connected to upstream <see cref="NodeOutput"/> instances.
        /// </summary>
        protected NodeInput[] InputConnections;
        
        /// <summary>
        /// Array of outputs available to be connected to downstream <see cref="NodeInput"/> instances.
        /// </summary>
        protected NodeOutput[] OutputConnections;

        /// <summary>
        /// Change how to execute the tasks.
        /// </summary>
        /// <param name="taskManager">Define how to execute the
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Tasks</see>.
        /// This allow to implement either a custom task manager or decide whether all the
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Tasks</see> should be
        /// executed on the main thread or be multi-threaded.</param>
        public void SetTaskManager(ITaskManager taskManager)
        {
            m_TaskManagerWrapper.SetTaskManager(taskManager);
        }

        /// <summary>
        /// Connect an <see cref="NodeOutput{T}"/> to an <see cref="NodeInput{T}"/> allowing the <paramref name="rightNode"/>
        /// retrieving the <paramref name="leftNode"/> value on execution.
        /// </summary>
        /// <param name="leftNode">Upstream node to connect.</param>
        /// <param name="rightNode">Downstream node to connect.</param>
        /// <typeparam name="T">Restrict the two connectors to be of this type.</typeparam>
        public static void Connect<T>(NodeOutput<T> leftNode, NodeInput<T> rightNode)
        {
            leftNode.ConnectedInput = rightNode;
            rightNode.ConnectedOutput = leftNode;
        }

    }
}
