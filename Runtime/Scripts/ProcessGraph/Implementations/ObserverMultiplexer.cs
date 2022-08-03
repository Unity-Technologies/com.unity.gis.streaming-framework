using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Multiplexer used to execute with <see cref="DetailObserverData"/> <see cref="UGProcessingNode.NodeInput">inputs</see>.
    /// </summary>
    public class ObserverMultiplexer : UGProcessingNode
    {
        /// <summary>
        /// Class for a single <see cref="DetailObserverData"/> <see cref="NodeInput{T}"/>.
        /// </summary>
        public class Input : NodeInput<DetailObserverData>
        {
            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="mux">Owner of this input.</param>
            /// <param name="index"><see cref="NodeInput{T}"/> index in the list of <see cref="DetailObserverData"/> inputs.</param>
            public Input(ObserverMultiplexer mux, int index)
            {
                m_Index = index;
                m_Mux = mux;
            }

            private readonly int m_Index;
            
            private readonly ObserverMultiplexer m_Mux;
            
            /// <inheritdoc cref="NodeInput{T}.IsReadyForData"/> 
            public override bool IsReadyForData
            {
                get { return true; }
            }

            /// <inheritdoc cref="NodeInput{T}.ProcessData(ref T)"/> 
            public override void ProcessData(ref DetailObserverData data)
            {
                m_Mux.ProcessData(m_Index, ref data);
            }
        }

        private readonly Input[] m_Inputs;
        
        private DetailObserverData[] m_Data;

        /// <summary>
        /// Get the result of this instance once <see cref="ProcessData"/> has been executed.
        /// </summary>
        public NodeOutput<DetailObserverData[]> Output { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="inputCount">Amount of <see cref="DetailObserverData"/> to be gathered for the <see cref="Output"/>.</param>
        public ObserverMultiplexer(int inputCount)
        {
            InputConnections = m_Inputs = new Input[inputCount];
            for (int i = 0; i < m_Inputs.Length; i++)
            {
                m_Inputs[i] = new Input(this, i);
            }

            m_Data = new DetailObserverData[inputCount];

            Output = new NodeOutput<DetailObserverData[]>(this);
            OutputConnections = new NodeOutput[] { Output };
        }

        /// <inheritdoc cref="UGProcessingNode.Dispose"/> 
        public override void Dispose()
        {
            //
            //  Method intentionally left blank
            //
        }

        /// <summary>
        /// Get the <see cref="DetailObserverData"/> <see cref="UGProcessingNode.NodeInput{T}"/> for the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of the instance to retrieve.</param>
        /// <returns>The node input instance at the given <paramref name="index"/>.</returns>
        public Input GetInput(int index)
        {
            Assert.IsTrue(index < m_Inputs.Length);
            return m_Inputs[index];
        }

        /// <summary>
        /// Execute the the upstream <see cref="Input"/> and set its value to the <see cref="UGProcessingNode.NodeOutput{T}"/>
        /// at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Store the result at this index in the <see cref="Output"/> value.</param>
        /// <param name="data">Store this instance in the <see cref="Output"/> value.</param>
        private void ProcessData(int index, ref DetailObserverData data)
        {
            Assert.IsTrue(index < m_Inputs.Length);
            m_Data[index] = data;
        }
        
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

        /// <inheritdoc cref="UGProcessingNode.MainThreadUpKeep"/> 
        public override void MainThreadUpKeep()
        {
            if (Output.IsReadyForData)
                Output.ProcessData(ref m_Data);
        }

        /// <inheritdoc cref="UGProcessingNode.MainThreadProcess"/> 
        public override void MainThreadProcess()
        {
            //
            //  Method intentionally left blank
            //
        }
    }
}
