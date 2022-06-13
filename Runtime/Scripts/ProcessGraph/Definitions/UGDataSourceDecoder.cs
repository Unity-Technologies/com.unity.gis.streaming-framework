
namespace Unity.Geospatial.Streaming
{

    //
    //  TODO - Expand definition to accept and manage multiple inputs
    //
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

        protected UGDataSourceDecoder(int outputCount)
        {
            InputConnections = new NodeInput[1];
            InputConnections[0] = Input = new DetailObserverInput(this);


            m_Outputs = new NodeOutput<InstanceCommand>[outputCount];
            for (int i = 0; i < outputCount; i++)
                m_Outputs[i] = new NodeOutput<InstanceCommand>(this);
            OutputConnections = m_Outputs;

        }

        public NodeInput<DetailObserverData[]> Input { get; private set; }
        
        private readonly NodeOutput<InstanceCommand>[] m_Outputs;
        
        public abstract void SetDetailObserverData(DetailObserverData[] data);

        protected override DataAvailability GetDataAvailabilityStatus(NodeOutput output)
        {
            return IsProcessing ? DataAvailability.Processing : DataAvailability.Idle;
        }

        public int OutputCount
        {
            get { return m_Outputs.Length; }
        }

        public NodeOutput<InstanceCommand> GetOutput(int index)
        {
            return m_Outputs[index];
        }
    }

}
