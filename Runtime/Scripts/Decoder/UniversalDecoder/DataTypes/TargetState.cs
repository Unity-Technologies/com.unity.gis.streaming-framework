
namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// This is the Target State of a node.
    /// </summary>
    public readonly struct TargetState
    {
        /// <summary>
        /// Byte value of collapsed
        /// </summary>
        private const byte k_Collapsed = 0x00;

        /// <summary>
        /// Byte value of expanded
        /// </summary>
        private const byte k_Expanded = 0x01;

        /// <summary>
        /// The internal state of the target state
        /// </summary>
        private readonly byte m_State;

        /// <summary>
        /// Default constructor. Can't be used. Instead, use the
        /// predefined <see cref="Collapsed"/> and <see cref="Expanded"/>
        /// </summary>
        /// <param name="state"></param>
        private TargetState(byte state)
        {
            m_State = state;
        }

        /// <summary>
        /// The collapsed state
        /// </summary>
        public static readonly TargetState Collapsed = new TargetState(k_Collapsed);

        /// <summary>
        /// The expanded state
        /// </summary>
        public static readonly TargetState Expanded = new TargetState(k_Expanded);

        /// <summary>
        /// Returns whether the current state is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return m_State == k_Expanded; }
        }

        /// <summary>
        /// Returns whether the current state is collapsed
        /// </summary>
        public bool IsCollapsed
        {
            get { return m_State == k_Collapsed; }
        }

        /// <summary>
        /// Generate a human readable string to represent the currents state.
        /// </summary>
        /// <returns>Human readable string of the current state.</returns>
        public override string ToString()
        {
            return $"TargetState({(IsExpanded ? "Expanded" : "Collapsed")})";
        }


    }

}
