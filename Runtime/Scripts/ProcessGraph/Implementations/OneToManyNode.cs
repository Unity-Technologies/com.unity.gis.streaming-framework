
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Create a many output as requested pointing to the same input value.
    /// </summary>
    /// <typeparam name="T">Type of value receiving in the input and sharing through the output.</typeparam>
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

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="outputCount">Create this amount of outputs that can be retrieve via <see cref="GetOutput(int)"/></param>
        public OneToManyNode(int outputCount)
        {
            Input = new InputImpl(this);
            InputConnections = new NodeInput<T>[] { Input };

            OutputConnections = m_Outputs = new NodeOutput<T>[outputCount];
            for (int i = 0; i < outputCount; ++i)
                m_Outputs[i] = new NodeOutput<T>(this);
        }

        /// <summary>
        /// The input to be converted to multiple outputs.
        /// </summary>
        public NodeInput<T> Input { get; private set; }
        
        private readonly NodeOutput<T>[] m_Outputs;
        
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
            //  Intentionnally left blank
            //
        }

        /// <inheritdoc cref="UGProcessingNode.MainThreadProcess"/> 
        public override void MainThreadProcess()
        {
            throw new System.NotImplementedException("This should never be called");
        }

        /// <inheritdoc cref="UGProcessingNode.MainThreadUpKeep"/> 
        public override void MainThreadUpKeep()
        {
            //
            //  Intentionnally left blank
            //
        }

        /// <summary>
        /// Get the number of available outputs that can be retrieved from <see cref="GetOutput(int)"/>.
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
        public NodeOutput<T> GetOutput(int index)
        {
            return m_Outputs[index];
        }
    }
}
