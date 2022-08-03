
namespace Unity.Geospatial.Streaming.UniversalDecoder
{

    /// <summary>
    /// This is the underlying node which is stored within the <see cref="BoundingVolumeHierarchy{T}"/>.
    /// </summary>
    /// <typeparam name="T">Must match the type of the <see cref="BoundingVolumeHierarchy{T}"/></typeparam>
    internal struct Node<T> where T : struct
    {
        /// <summary>
        /// Data of the <see cref="Node{T}"/> either retrieved from the <see cref="NodeContent"/> or an other streamed source.
        /// </summary>
        public NodeData Data { get; set; }

        /// <summary>
        /// Directives on how the <see cref="Node{T}"/> should be accessed and loaded.
        /// </summary>
        public NodeContent Content { get; set; }

        /// <summary>
        /// <see cref="NodeId.Id"/> of the <see cref="Node{T}"/> parent of this instance. This allow to navigate the
        /// in reverse order (child to parent).
        /// </summary>
        public int Parent { get; set; }

        /// <summary>
        /// Id allowing to retrieve the children of the <see cref="Node{T}"/> via an <see cref="IndirectionBlock"/>.
        /// </summary>
        public int ChildrenBlockId { get; set; }

        /// <summary>
        /// Level of the <see cref="Node{T}"/> in the hierarchy.
        /// Lower the number, closer to the root <see cref="Node{T}"/>;
        /// Higher the number, deeper in the hierarchy the <see cref="Node{T}"/> is.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// The actual state of the <see cref="Node{T}"/>.
        /// </summary>
        public CurrentState CurrentState { get; set; }

        /// <summary>
        /// The expected state of the <see cref="Node{T}"/>.
        /// </summary>
        public TargetState TargetState { get; set; }

        /// <summary>
        /// To be <see cref="UniversalDecoder.CurrentState.Loaded"/>, the <see cref="BoundingVolumeHierarchy{T}.GetErrorSpecification"/> result
        /// for this node must be below this value.
        /// </summary>
        public float ErrorSpecification { get; set; }

        /// <summary>
        /// Node-specific memory cache for the scheduler to store intermediate values.
        /// </summary>
        public T SchedulerCache { get; set; }

        /// <summary>
        /// Constructor for default node
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private Node(int parent, int childrenBlockId, int level, float errorSpecification)
        {
            Parent = parent;
            ChildrenBlockId = childrenBlockId;
            Level = level;
            ErrorSpecification = errorSpecification;

            Data = default;
            Content = null;
            CurrentState = default;
            TargetState = default;
            SchedulerCache = default;
        }

        /// <summary>
        /// <see cref="Node{T}"/> creation with default value set.
        /// </summary>
        /// <returns>A new <see cref="Node{T}"/> <see lanword="struct"/>.</returns>
        public static Node<T> Default
        {
            get
            {
                return new Node<T>(NodeId.NullID, -1, -1, float.MaxValue);
            }
        }

    }

}
