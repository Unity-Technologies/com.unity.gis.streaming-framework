
namespace Unity.Geospatial.Streaming
{
    public struct InstanceCommand
    {
        public enum CommandType
        { 
            Allocate,
            Dispose,
            UpdateVisibility,
            BeginAtomic,
            EndAtomic
        }

        public InstanceCommand(CommandType command, InstanceID id, InstanceData data, bool visibility)
        {
            Command = command;
            Id = id;
            Data = data;
            Visibility = visibility;
        }

        public CommandType Command { get; set; }

        public InstanceID Id { get; set; }

        public InstanceData Data { get; set; }

        public bool Visibility { get; set; }

        public static InstanceCommand Allocate(InstanceID id, InstanceData data)
        {
            return new InstanceCommand(
                CommandType.Allocate,
                id,
                data,
                default);
        }

        public static InstanceCommand Dispose(InstanceID id)
        {
            return new InstanceCommand(
                CommandType.Dispose,
                id,
                default,
                default);
        }

        public static InstanceCommand UpdateVisibility(InstanceID id, bool isVisible)
        {
            return new InstanceCommand(
                CommandType.UpdateVisibility,
                id,
                default,
                isVisible);
        }

        public static InstanceCommand BeginAtomic()
        {
            return new InstanceCommand(
                CommandType.BeginAtomic,
                default,
                default,
                default);
        }

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
