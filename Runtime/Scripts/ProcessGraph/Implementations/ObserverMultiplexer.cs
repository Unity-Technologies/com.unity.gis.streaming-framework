using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    public class ObserverMultiplexer : UGProcessingNode
    {
        public class Input : NodeInput<DetailObserverData>
        {
            public Input(ObserverMultiplexer mux, int index)
            {
                m_Index = index;
                m_Mux = mux;
            }

            private readonly int m_Index;
            
            private readonly ObserverMultiplexer m_Mux;

            public override bool IsReadyForData
            {
                get { return true; }
            }

            public override void ProcessData(ref DetailObserverData data)
            {
                m_Mux.ProcessData(m_Index, ref data);
            }
        }

        private readonly Input[] m_Inputs;
        
        private DetailObserverData[] m_Data;

        public NodeOutput<DetailObserverData[]> Output { get; private set; }

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

        public override void Dispose()
        {
            //
            //  Method intentionally left blank
            //
        }

        public Input GetInput(int index)
        {
            Assert.IsTrue(index < m_Inputs.Length);
            return m_Inputs[index];
        }

        private void ProcessData(int index, ref DetailObserverData data)
        {
            Assert.IsTrue(index < m_Inputs.Length);
            m_Data[index] = data;
        }
        
        public override bool ScheduleMainThread
        {
            get { return false; }
        }

        protected override bool IsProcessing
        {
            get { return false; }
        }

        public override void MainThreadUpKeep()
        {
            if (Output.IsReadyForData)
                Output.ProcessData(ref m_Data);
        }

        public override void MainThreadProcess()
        {
            //
            //  Method intentionally left blank
            //
        }

        

        
    }

}
