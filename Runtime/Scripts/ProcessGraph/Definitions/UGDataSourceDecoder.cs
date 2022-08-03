
namespace Unity.Geospatial.Streaming
{
    //
    //  TODO - Expand definition to accept and manage multiple inputs
    //
    /// <summary>
    /// Base <see cref="UGProcessingNode"/> class allowing to load a dataset.
    /// </summary>
    public abstract class UGDataSourceDecoder : UGProcessingNode
    {
        private sealed class DetailObserverInput : NodeInput<DetailObserverData[]>
        {
            public DetailObserverInput(UGDataSourceDecoder decoder)
            {
                m_Decoder = decoder;
            }

            private readonly UGDataSourceDecoder m_Decoder;

            public override bool IsReadyForData
            {
                get { return true; }
            }

            public override void ProcessData(ref DetailObserverData[] data)
            {
                m_Decoder.SetDetailObserverData(data);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="outputCount">Specify the amount of <see cref="InstanceCommand"/> that will be used as
        /// <see cref="UGProcessingNode.NodeOutput{T}">NodeOutputs</see>.</param>
        protected UGDataSourceDecoder(int outputCount)
        {
            InputConnections = new NodeInput[1];
            InputConnections[0] = Input = new DetailObserverInput(this);


            m_Outputs = new NodeOutput<InstanceCommand>[outputCount];
            for (int i = 0; i < outputCount; i++)
                m_Outputs[i] = new NodeOutput<InstanceCommand>(this);
            OutputConnections = m_Outputs;

        }

        /// <summary>
        /// Array of <see cref="DetailObserverData"/> allowing to calculate the lowest geometric error against them.
        /// </summary>
        public NodeInput<DetailObserverData[]> Input { get; private set; }
        
        private readonly NodeOutput<InstanceCommand>[] m_Outputs;
        
        /// <summary>
        /// Apply the <see cref="DetailObserverData">observers</see> allowing to use calculate the geometric error against them.
        /// </summary>
        /// <param name="data">Replace the actual observers with those.</param>
        public abstract void SetDetailObserverData(DetailObserverData[] data);

        /// <inheritdoc cref="UGProcessingNode.GetDataAvailabilityStatus"/>
        protected override DataAvailability GetDataAvailabilityStatus(NodeOutput output)
        {
            return IsProcessing ? DataAvailability.Processing : DataAvailability.Idle;
        }

        /// <summary>
        /// Get the number of <see cref="UGProcessingNode.NodeOutput{T}"/> have been created with a <see cref="InstanceCommand"/>.
        /// </summary>
        public int OutputCount
        {
            get { return m_Outputs.Length; }
        }

        /// <summary>
        /// Get the <see cref="InstanceCommand"/> <see cref="UGProcessingNode.NodeOutput{T}"/> for the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of the instance to retrieve.</param>
        /// <returns>The node output instance at the given <paramref name="index"/>.</returns>
        public NodeOutput<InstanceCommand> GetOutput(int index)
        {
            return m_Outputs[index];
        }
    }

}
