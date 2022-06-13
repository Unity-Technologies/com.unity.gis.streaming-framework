using System.Collections.Generic;

using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{

    public class DecoderMultiplexer :
        UGProcessingNode
    {
        private sealed class Input :
            NodeInput<InstanceCommand>
        {
            public Input (DecoderMultiplexer demux, int index)
            {
                m_Index = index;
                m_Demux = demux;
            }

            private readonly int m_Index;
            private readonly DecoderMultiplexer m_Demux;

            public override bool IsReadyForData
            {
                get { return m_Demux.IsReadyForData(m_Index); }
            }

            public override void ProcessData(ref InstanceCommand data)
            {
                m_Demux.ProcessData(m_Index, ref data);
            }
        }

        public DecoderMultiplexer(int inputCount)
        {
            m_InputCount = inputCount;
            var inputQueues = new Queue<InstanceCommand>[inputCount];
            for (int i = 0; i < inputCount; i++)
                inputQueues[i] = new Queue<InstanceCommand>();

            InputConnections = m_Inputs = new NodeInput<InstanceCommand>[inputCount];
            for (int i = 0; i < m_Inputs.Length; ++i)
                m_Inputs[i] = new Input(this, i);

            Output = new NodeOutput<InstanceCommand>(this);
            OutputConnections = new NodeOutput[]{ Output };
        }

        private bool m_IsAtomic;

        private readonly int m_InputCount;

        private int m_RoundRobinIndex;

        private readonly NodeInput<InstanceCommand>[] m_Inputs;
        public NodeOutput<InstanceCommand> Output { get; private set; }

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
            //  Intentionally left blank
            //
        }

        private void ProcessData(int index, ref InstanceCommand data)
        {
            Assert.AreEqual(index, m_RoundRobinIndex, "Data passed to multiplexer when it wasn't it's turn");

            if (data.Command == InstanceCommand.CommandType.BeginAtomic)
            {
                Assert.IsFalse(m_IsAtomic);
                m_IsAtomic = true;
            }
            else if (data.Command == InstanceCommand.CommandType.EndAtomic)
            {
                Assert.IsTrue(m_IsAtomic);
                m_IsAtomic = false;
            }

            Output.ProcessData(ref data);

            if (!m_IsAtomic)
                IncrementRoundRobin();
        }

        private void IncrementRoundRobin()
        {
            Assert.IsFalse(m_IsAtomic);

            m_RoundRobinIndex++;
            if (m_RoundRobinIndex >= m_InputCount)
                m_RoundRobinIndex = 0;
        }

        private bool IsReadyForData(int index)
        {
            while (!m_IsAtomic && index != m_RoundRobinIndex && InputConnections[m_RoundRobinIndex].UpstreamDataAvailability == DataAvailability.Idle)
                IncrementRoundRobin();

            return (index == m_RoundRobinIndex) && Output.IsReadyForData;
        }

        public override void MainThreadUpKeep()
        {
            //
            //  Intentionally left blank
            //
        }

        public override void MainThreadProcess()
        {
            throw new System.NotImplementedException("This should never be called");
        }

        public int InputCount
        {
            get { return m_Inputs.Length; }
        }

        public NodeInput<InstanceCommand> GetInput(int index)
        {
            return m_Inputs[index];
        }
    }

}
