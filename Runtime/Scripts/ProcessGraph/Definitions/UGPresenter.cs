using System.Collections.Generic;

using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
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
        
        protected UGPresenter()
        {
            OutputConnections = null;
            InputConnections = new NodeInput[1];
            InputConnections[0] = Input = new NodeInputImpl(this);
        }

        public NodeInput<InstanceCommand> Input { get; private set; }
        private bool m_IsAtomicInProgress;
        private readonly Queue<InstanceCommand> m_AtomicQueue = new Queue<InstanceCommand>();

        protected override bool IsProcessing
        {
            get { return false; }
        }

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

        protected abstract void CmdAllocate(InstanceID id, InstanceData instance);
        protected abstract void CmdDispose(InstanceID id);
        protected abstract void CmdUpdateVisibility(InstanceID id, bool isVisible);

        public override void MainThreadProcess()
        {
            throw new System.InvalidOperationException("This method should never be called");
        }

        public override void MainThreadUpKeep()
        {
            //
            //  Method left intentionnally blank
            //
        }
    }
}
