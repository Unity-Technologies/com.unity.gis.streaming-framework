using System.Collections.Generic;

using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Responsible to convert the streamed geometry into actual
    /// <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObjects</see>. Most
    /// configurations will only require a single presenter but applications with multiple cameras or applications
    /// where the source data is not normalized in space (think multiple planets or non-geolocated dataset) may
    /// require multiple presenters.
    /// </summary>
    public abstract class UGPresenter : UGProcessingNode
    {
        private sealed class NodeInputImpl : NodeInput<InstanceCommand>
        {
            public NodeInputImpl(UGPresenter presenter)
            {
                m_Presenter = presenter;
            }

            private readonly UGPresenter m_Presenter;

            public override bool IsReadyForData
            {
                get { return true; }
            }

            public override void ProcessData(ref InstanceCommand data)
            {
                m_Presenter.ProcessData(ref data);
            }
        }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected UGPresenter()
        {
            OutputConnections = null;
            InputConnections = new NodeInput[1];
            InputConnections[0] = Input = new NodeInputImpl(this);
        }

        /// <summary>
        /// <see cref="InstanceCommand"/> to be executed before this instance gets executed.
        /// </summary>
        public NodeInput<InstanceCommand> Input { get; private set; }
        
        private bool m_IsAtomicInProgress;
        
        private readonly Queue<InstanceCommand> m_AtomicQueue = new Queue<InstanceCommand>();

        /// <inheritdoc cref="UGProcessingNode.IsProcessing"/> 
        protected override bool IsProcessing
        {
            get { return false; }
        }

        /// <inheritdoc cref="UGProcessingNode.ScheduleMainThread"/> 
        public override bool ScheduleMainThread
        {
            get { return false; }
        }

        private void ProcessData(ref InstanceCommand command)
        {
            if (command.Command == InstanceCommand.CommandType.BeginAtomic)
            {
                Assert.IsFalse(m_IsAtomicInProgress);
                m_IsAtomicInProgress = true;
            }
            else if (command.Command == InstanceCommand.CommandType.EndAtomic)
            {
                Assert.IsTrue(m_IsAtomicInProgress);
                m_IsAtomicInProgress = false;
                while (m_AtomicQueue.Count > 0)
                    ProcessDataParcel(m_AtomicQueue.Dequeue());
            }
            else if (m_IsAtomicInProgress)
            {
                m_AtomicQueue.Enqueue(command);
            }
            else
            {
                ProcessDataParcel(command);
            }
        }

        private void ProcessDataParcel(InstanceCommand command)
        {
            switch (command.Command)
            {
                case InstanceCommand.CommandType.Allocate:
                    CmdAllocate(command.Id, command.Data);
                    break;

                case InstanceCommand.CommandType.Dispose:
                    CmdDispose(command.Id);
                    break;

                case InstanceCommand.CommandType.UpdateVisibility:
                    CmdUpdateVisibility(command.Id, command.Visibility);
                    break;

                default:
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Command to be executed when an instance gets loaded.
        /// </summary>
        /// <param name="instanceId">Id of the instance that is loaded.</param>
        /// <param name="instanceData">Data loaded for the given <paramref name="instanceId"/>.</param>
        protected abstract void CmdAllocate(InstanceID instanceId, InstanceData instanceData);
        
        /// <summary>
        /// Command to be executed when an instance gets unloaded.
        /// </summary>
        /// <param name="instanceId">Id of the instance that is disposed.</param>
        protected abstract void CmdDispose(InstanceID instanceId);
        
        /// <summary>
        /// Command to be executed when the visibility state for the given <paramref name="instanceId">instance</paramref> changes.
        /// </summary>
        /// <param name="instanceId">Instance with its visibility state changed.</param>
        /// <param name="visibility">
        /// <see langword="true"/> when the instance is displayed;
        /// <see langword="false"/> when the instance is hidden.
        /// </param>
        protected abstract void CmdUpdateVisibility(InstanceID instanceId, bool visibility);

        /// <inheritdoc cref="UGProcessingNode.MainThreadProcess"/> 
        public override void MainThreadProcess()
        {
            throw new System.InvalidOperationException("This method should never be called");
        }

        /// <inheritdoc cref="UGProcessingNode.MainThreadUpKeep"/> 
        public override void MainThreadUpKeep()
        {
            //
            //  Method left intentionally blank
            //
        }
    }
}
