using System.Collections.Generic;

using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Multiplexer used to converge multiple outputs into a single input with round-robin arbitration of which input
    /// to use at which time.
    /// </summary>
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

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="inputCount">Amount of decoders to be executed.</param>
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
        
        /// <summary>
        /// Downstream <see cref="UGProcessingNode"/> to be executed after this instance.
        /// </summary>
        public NodeOutput<InstanceCommand> Output { get; private set; }

        /// <inheritdoc cref="UGProcessingNode.ScheduleMainThread"/> 
        public override bool ScheduleMainThread
        {
            get { return false; }
        }

        /// <inheritdoc cref="UGProcessingNode.IsProcessing"/> 
        protected override bool IsProcessing
        {
            get { return false; }
        }

        /// <inheritdoc cref="UGProcessingNode.Dispose"/> 
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

        /// <inheritdoc cref="UGProcessingNode.MainThreadUpKeep"/> 
        public override void MainThreadUpKeep()
        {
            //
            //  Intentionally left blank
            //
        }

        /// <inheritdoc cref="UGProcessingNode.MainThreadProcess"/> 
        public override void MainThreadProcess()
        {
            throw new System.NotImplementedException("This should never be called");
        }

        /// <summary>
        /// Get the number of <see cref="UGProcessingNode.NodeInput{T}"/> have been created with a <see cref="InstanceCommand"/>.
        /// </summary>
        public int InputCount
        {
            get { return m_Inputs.Length; }
        }
        
        /// <summary>
        /// Get the <see cref="InstanceCommand"/> <see cref="UGProcessingNode.NodeInput{T}"/> for the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of the instance to retrieve.</param>
        /// <returns>The node input instance at the given <paramref name="index"/>.</returns>
        public NodeInput<InstanceCommand> GetInput(int index)
        {
            return m_Inputs[index];
        }
    }

}
