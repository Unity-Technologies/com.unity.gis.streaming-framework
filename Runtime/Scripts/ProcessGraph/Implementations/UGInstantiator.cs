using System;

namespace Unity.Geospatial.Streaming
{
    //
    //  TODO - Change input to mutable instance
    //
    public class UGInstantiator : UGProcessingNode
    {
        private sealed class NodeInputImpl : NodeInput<InstanceCommand>
        {
            public NodeInputImpl(UGInstantiator instantiator)
            {
                m_Instantiator = instantiator;
            }

            private readonly UGInstantiator m_Instantiator;

            public override bool IsReadyForData
            {
                get { return m_Instantiator.IsReadyForData; }
            }

            public override void ProcessData(ref InstanceCommand data)
            {
                m_Instantiator.ProcessData(ref data);
            }
        }

        public UGInstantiator(int outputSize)
        {

            InputConnections = new NodeInput[1];
            InputConnections[0] = Input = new NodeInputImpl(this);

            m_Outputs = new NodeOutput<InstanceCommand>[outputSize];
            for (int i = 0; i < outputSize; i++)
                m_Outputs[i] = new NodeOutput<InstanceCommand>(this);
            OutputConnections = m_Outputs;
        }

        public NodeInput<InstanceCommand> Input { get; private set; }
        private readonly NodeOutput<InstanceCommand>[] m_Outputs;

        public int OutputCount
        {
            get { return m_Outputs.Length; }
        }

        public NodeOutput<InstanceCommand> GetOutput(int index)
        {
            return m_Outputs[index];
        }

        private bool IsReadyForData
        {
            get
            {
                foreach (var output in m_Outputs)
                {
                    if (!output.IsReadyForData)
                        return false;
                }

                return true;
            }
        }

        private void ProcessData(ref InstanceCommand data)
        {
            foreach (var output in m_Outputs)
                output.ProcessData(ref data);
        }

        public override bool ScheduleMainThread
        {
            get { return false; }
        }

        protected override bool IsProcessing
        {
            get { return false; }
        }

        public override void Dispose()
        {
            //
            //  Blank method
            //
        }

        public override void MainThreadUpKeep()
        {
            //
            //  Blank method
            //
        }

        public override void MainThreadProcess()
        {
            throw new InvalidOperationException("This method should never be called");
        }
    }

}
