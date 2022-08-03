
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Directives executed by the <see cref="UGCommandBufferProcessor"/> when allocating a new node instance.
    /// Allow to load in memory the geometry and textures before displaying them.
    /// </summary>
    public struct InstanceCommand
    {
        /// <summary>
        /// Possible commands to be executed for a given <see cref="InstanceID"/>.
        /// </summary>
        public enum CommandType
        { 
            /// <summary>
            /// Allocate memory space for a new instance.
            /// </summary>
            Allocate,
            
            /// <summary>
            /// Unload an instance, it won't be available afterward.
            /// </summary>
            Dispose,
            
            /// <summary>
            /// Change the visibility state of the instance (visible / hidden)
            /// </summary>
            UpdateVisibility,
            
            /// <summary>
            /// Specify a new block of commands will be requested.
            /// </summary>
            BeginAtomic,
            
            /// <summary>
            /// Specify the last block of commands have been completed.
            /// </summary>
            EndAtomic
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="command">Command to be executed for a given <see cref="InstanceID"/>.</param>
        /// <param name="id">Instance to execute the command on.</param>
        /// <param name="data">Data to be loaded when the instance is visible.</param>
        /// <param name="visibility">
        /// <see langword="true"/> to set to display the instance;
        /// <see langword="false"/> to hide it.
        /// </param>
        public InstanceCommand(CommandType command, InstanceID id, InstanceData data, bool visibility)
        {
            Command = command;
            Id = id;
            Data = data;
            Visibility = visibility;
        }

        /// <summary>
        /// Command to be executed for a given <see cref="InstanceID"/>.
        /// </summary>
        public CommandType Command { get; set; }

        /// <summary>
        /// Instance to execute the command on.
        /// </summary>
        public InstanceID Id { get; set; }

        /// <summary>
        /// Data to be loaded when the instance is visible.
        /// </summary>
        public InstanceData Data { get; set; }

        /// <summary>
        /// <see langword="true"/> to set to display the instance;
        /// <see langword="false"/> to hide it.
        /// </summary>
        public bool Visibility { get; set; }
        
        /// <summary>
        /// Create an instance by linking a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>,
        /// a <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> and
        /// a <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> together
        /// </summary>
        /// <param name="instanceId">Id of the instance allowing to refer to the command / loaded instance.</param>
        /// <param name="instanceData">Information of what to link together.</param>
        /// <returns>A new <see cref="InstanceCommand"/> filled the command directives.</returns>
        public static InstanceCommand Allocate(InstanceID instanceId, InstanceData instanceData)
        {
            return new InstanceCommand(
                CommandType.Allocate,
                instanceId,
                instanceData,
                default);
        }

        /// <summary>
        /// Unload an instance by unlinking its <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>,
        /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> and
        /// <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see>.
        /// </summary>
        /// <param name="instanceId">Id of the instance to dispose of.</param>
        /// <returns>A new <see cref="InstanceCommand"/> filled the command directives.</returns>
        public static InstanceCommand Dispose(InstanceID instanceId)
        {
            return new InstanceCommand(
                CommandType.Dispose,
                instanceId,
                default,
                default);
        }

        /// <summary>
        /// Change the visibility state for the given <paramref name="instanceId">instance</paramref>.
        /// </summary>
        /// <param name="instanceId">Instance to change its visibility state.</param>
        /// <param name="visibility">
        /// <see langword="true"/> to set to display the instance;
        /// <see langword="false"/> to hide it.
        /// </param>
        /// <returns>A new <see cref="InstanceCommand"/> filled the command directives.</returns>
        public static InstanceCommand UpdateVisibility(InstanceID instanceId, bool visibility)
        {
            return new InstanceCommand(
                CommandType.UpdateVisibility,
                instanceId,
                default,
                visibility);
        }

        /// <summary>
        /// Specify a new block of commands will be requested.
        /// </summary>
        /// <returns>A new <see cref="InstanceCommand"/> filled the command directives.</returns>
        public static InstanceCommand BeginAtomic()
        {
            return new InstanceCommand(
                CommandType.BeginAtomic,
                default,
                default,
                default);
        }

        /// <summary>
        /// Specify the last block of commands have been completed.
        /// </summary>
        /// <returns>A new <see cref="InstanceCommand"/> filled the command directives.</returns>
        public static InstanceCommand EndAtomic()
        {
            return new InstanceCommand(
                CommandType.EndAtomic,
                default,
                default,
                default);
        }
    }
}
