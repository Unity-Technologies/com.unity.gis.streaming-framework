using System;

namespace Unity.Geospatial.Streaming
{
    //
    //  TODO - Change input to mutable instance
    //
    /// <summary>
    /// Allow to chain <see cref="InstanceCommand"/> results.
    /// </summary>
    public class UGInstantiator : UGProcessingNode
    {
        private sealed class NodeInputImpl : NodeInput<InstanceCommand>
        {
            public NodeInputImpl(UGInstantiator instantiator)
            {
                m_Instantiator = instantiator;
            }

            private readonly UGInstantiator m_Instantiator;

            /// <inheritdoc cref="NodeInput{T}.IsReadyForData"/> 
            public override bool IsReadyForData
            {
                get { return m_Instantiator.IsReadyForData; }
            }

            /// <inheritdoc cref="NodeInput{T}.ProcessData"/> 
            public override void ProcessData(ref InstanceCommand data)
            {
                m_Instantiator.ProcessData(ref data);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="outputSize">Amount of <see cref="UGProcessingNode.NodeOutput{T}"/> to create for this instance.</param>
        public UGInstantiator(int outputSize)
        {

            InputConnections = new NodeInput[1];
            InputConnections[0] = Input = new NodeInputImpl(this);

            m_Outputs = new NodeOutput<InstanceCommand>[outputSize];
            for (int i = 0; i < outputSize; i++)
                m_Outputs[i] = new NodeOutput<InstanceCommand>(this);
            OutputConnections = m_Outputs;
        }

        /// <summary>
        /// Execute this <see cref="InstanceCommand"/> before executing the output commands.
        /// </summary>
        public NodeInput<InstanceCommand> Input { get; private set; }
        
        private readonly NodeOutput<InstanceCommand>[] m_Outputs;

        /// <summary>
        /// Amount of available <see cref="UGProcessingNode.NodeOutput{T}"/>.
        /// </summary>
        public int OutputCount
        {
            get { return m_Outputs.Length; }
        }

        /// <summary>
        /// Get the <see cref="UGProcessingNode.NodeOutput{T}"/> for the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of the instance to retrieve.</param>
        /// <returns>The node output instance at the given <paramref name="index"/>.</returns>
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
            //  Blank method
            //
        }

        /// <inheritdoc cref="UGProcessingNode.MainThreadUpKeep"/> 
        public override void MainThreadUpKeep()
        {
            //
            //  Blank method
            //
        }

        /// <inheritdoc cref="UGProcessingNode.MainThreadProcess"/> 
        public override void MainThreadProcess()
        {
            throw new InvalidOperationException("This method should never be called");
        }
    }

}
