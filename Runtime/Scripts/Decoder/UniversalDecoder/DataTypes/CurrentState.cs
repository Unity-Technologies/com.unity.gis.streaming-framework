
namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// This struct is used to determine the current state of a node in
    /// the BoundingVolumeHierarchy. It consists of a bit field which
    /// is used to determine loaded and visibility states.
    /// </summary>
    public readonly struct CurrentState
    {
        /// <summary>
        /// Can be used to initialize the current state
        /// </summary>
        /// <example>
        /// new CurrentState(CurrentState.Unloaded | CurrentState.Hidden);
        /// </example>
        public const byte Unloaded = 0x00;

        /// <summary>
        /// Can be used to initialize the current state
        /// </summary>
        /// <example>
        /// new CurrentState(CurrentState.Unloaded | CurrentState.Hidden);
        /// </example>
        public const byte Hidden = 0x00;

        /// <summary>
        /// Can be used to initialize the current state
        /// </summary>
        /// <example>
        /// new CurrentState(CurrentState.Loaded | CurrentState.Hidden);
        /// </example>
        public const byte Loaded = 0x01 << 0;

        /// <summary>
        /// Can be used to initialize the current state
        /// </summary>
        /// <example>
        /// new CurrentState(CurrentState.Loaded | CurrentState.Visible);
        /// </example>
        public const byte Visible = 0x01 << 1;

        /// <summary>
        /// The internal state of the CurrentState. It is public in order to
        /// enable direct bit-wise computations on the bit field if necessary.
        /// </summary>
        public byte State { get; }


        /// <summary>
        /// This is the most efficient constructor of the CurrentState and should
        /// be prefered for creating fixed values. See example for usage.
        /// </summary>
        /// <param name="state">The state with which to initialize the CurrentState struct.</param>
        /// <example>
        /// new CurrentState(CurrentState.Loaded | CurrentState.Hidden)
        /// </example>
        public CurrentState(byte state)
        {
            State = state;
        }

        /// <summary>
        /// This is a less efficient constructor that allows direct conversion of
        /// boolean values into the state flags. It is less error-prone that the
        /// other proposed constructor.
        /// </summary>
        /// <param name="loaded">Whether or not the state is loaded.</param>
        /// <param name="visible">Whether or not the state is visible</param>
        public CurrentState(bool loaded, bool visible = false)
        {
            State = (byte)(
                    (loaded ? Unloaded : Loaded) |
                    (visible ? Hidden : Visible));
        }


        /// <summary>
        /// Returns whether the state is unloaded, independently from the visibility state.
        /// </summary>
        public bool IsUnloaded { get { return (State & Loaded) == 0; } }

        /// <summary>
        /// Returns whether the state is loaded, independently from the visibility state.
        /// </summary>
        public bool IsLoaded {get { return (State & Loaded) != 0;} }

        /// <summary>
        /// Returns whether the state is hidden, independently from the loaded state.
        /// </summary>
        public bool IsHidden { get { return (State & Visible) == 0; } }


        /// <summary>
        /// Returns whether the state is visibile, independently from the loaded state.
        /// </summary>
        public bool IsVisible { get { return (State & Visible) != 0; } }

        /// <summary>
        /// Take the current state, set the visible flag and return the modified state.
        /// </summary>
        /// <returns>The modified state</returns>
        public CurrentState SetVisible()
        {
            return new CurrentState((byte)(State | Visible));
        }

        /// <summary>
        /// Take the current state, clear the visible flag and return the modified state.
        /// </summary>
        /// <returns>The modified state</returns>
        public CurrentState SetHidden()
        {
            return new CurrentState((byte)(State & ~Visible));
        }

        /// <summary>
        /// Take the current state, set the loaded flag and return the modified state.
        /// </summary>
        /// <returns>The modified state</returns>
        public CurrentState SetLoaded()
        {
            return new CurrentState((byte)(State | Loaded));
        }

        /// <summary>
        /// Take the current state, clear the loaded flag and return the modified state.
        /// </summary>
        /// <returns>The modified state</returns>
        public CurrentState SetUnloaded()
        {
            return new CurrentState((byte)(State & ~Loaded));
        }

        /// <summary>
        /// Returns a string representation of the current state.
        /// </summary>
        /// <returns>A string representation of the current state.</returns>
        public override string ToString()
        {
            return $"CurrentState({(IsLoaded ? "Loaded" : "Unloaded")} | {(IsVisible ? "Visible" : "Hidden")})";
        }
    }
}
