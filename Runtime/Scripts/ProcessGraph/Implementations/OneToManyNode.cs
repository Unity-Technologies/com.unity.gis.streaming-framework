
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    public class OneToManyNode<T> : UGProcessingNode
    {
        private sealed class InputImpl : NodeInput<T>
        {
            public InputImpl(OneToManyNode<T> node)
            {
                m_Node = node;
            }

            private readonly OneToManyNode<T> m_Node;

            public override bool IsReadyForData
            {
                get
                {
                    foreach(NodeOutput<T> output in m_Node.m_Outputs)
                    {
                        if (!output.IsReadyForData)
                            return false;
                    }
                    return true;
                }
            }


            public override void ProcessData(ref T data)
            {
                foreach (NodeOutput<T> output in m_Node.m_Outputs)
                {
                    Assert.IsTrue(output.IsReadyForData);
                    output.ProcessData(ref data);
                }
            }
        }

        public OneToManyNode(int outputCount)
        {
            Input = new InputImpl(this);
            InputConnections = new NodeInput<T>[] { Input };

            OutputConnections = m_Outputs = new NodeOutput<T>[outputCount];
            for (int i = 0; i < outputCount; ++i)
                m_Outputs[i] = new NodeOutput<T>(this);
        }

        public NodeInput<T> Input { get; private set; }
        
        private readonly NodeOutput<T>[] m_Outputs;
        
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
            //  Intentionnally left blank
            //
        }

        public override void MainThreadProcess()
        {
            throw new System.NotImplementedException("This should never be called");
        }

        public override void MainThreadUpKeep()
        {
            //
            //  Intentionnally left blank
            //
        }

        public int OutputCount
        {
            get { return m_Outputs.Length; }
        }

        public NodeOutput<T> GetOutput(int index)
        {
            return m_Outputs[index];
        }
    }
}
