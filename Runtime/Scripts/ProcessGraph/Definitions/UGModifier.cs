
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Base <see cref="UGProcessingNode"/> class used to alter loaded datasets.
    /// </summary>
    public abstract class UGModifier : UGProcessingNode
    {
        //
        //  TODO - Change this to mutable instance
        //
        private sealed class InputImpl : NodeInput<InstanceCommand>
        {
            public InputImpl(UGModifier modifier)
            {
                m_Modifier = modifier;
            }

            private readonly UGModifier m_Modifier;

            public override bool IsReadyForData
            {
                get { return m_Modifier.IsReadyForData; }
            }

            public override void ProcessData(ref InstanceCommand data)
            {
                m_Modifier.ProcessData(ref data);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected UGModifier()
        {
            OutputConnections = new NodeOutput[1];
            OutputConnections[0] = Output = new NodeOutput<InstanceCommand>(this);

            InputConnections = new NodeInput[1];
            InputConnections[0] = Input = new InputImpl(this);
        }

        /// <summary>
        /// <see cref="InstanceCommand"/> to be executed before this instance gets executed.
        /// </summary>
        public NodeInput<InstanceCommand> Input { get; private set; }
        
        /// <summary>
        /// <see cref="InstanceCommand"/> to be executed after this instance gets executed.
        /// </summary>
        public NodeOutput<InstanceCommand> Output { get; protected set; }

        /// <summary>
        /// <see langword="true"/> if this instance can be executed;
        /// <see langword="false"/> if the <see cref="Input"/> dependency is either not connected or not executed.
        /// </summary>
        protected abstract bool IsReadyForData { get; }

        /// <summary>
        /// Execute this instance and update the <see cref="Output"/> value based on the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Execute the <see cref="UGProcessingNode"/> based on this given value.</param>
        protected abstract void ProcessData(ref InstanceCommand instance);
    }
}
