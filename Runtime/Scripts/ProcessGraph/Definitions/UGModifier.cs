
namespace Unity.Geospatial.Streaming
{
    
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

        protected UGModifier()
        {
            OutputConnections = new NodeOutput[1];
            OutputConnections[0] = Output = new NodeOutput<InstanceCommand>(this);

            InputConnections = new NodeInput[1];
            InputConnections[0] = Input = new InputImpl(this);
        }

        public NodeInput<InstanceCommand> Input { get; private set; }
        public NodeOutput<InstanceCommand> Output { get; protected set; }

        protected abstract bool IsReadyForData { get; }
        protected abstract void ProcessData(ref InstanceCommand instance);
    }
}
