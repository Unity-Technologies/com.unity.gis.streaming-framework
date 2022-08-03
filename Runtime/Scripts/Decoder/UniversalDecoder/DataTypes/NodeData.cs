
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Data associated to a specific <see cref="NodeId"/>.
    /// The data refer to any information on how the node should be loaded without any notion on the hierarchy.
    /// For hierarchy information, see <see cref="Node{T}"/>.
    /// </summary>
    public readonly struct NodeData
    {
        /// <summary>
        /// Get if the node should always be expanded without calculating the <see cref="GeometricError"/>;
        /// <see langword="false"/> if the <see cref="GeometricError"/> should always be calculated when
        /// <see cref="TargetState.Expanded"/> state needs to be evaluated.
        /// </summary>
        public bool AlwaysExpand { get; }

        /// <summary>
        /// Limits of the node geometry allowing to calculate the intersection between the node geometry
        /// with the <see cref="UGSceneObserver"/> bounds.
        /// </summary>
        public DoubleBounds Bounds { get; }

        /// <summary>
        /// Maximum allowed geometric error indicating if the node should be loaded.
        /// Usually correspond to the screen space the node should have before being loaded.
        /// A high error will dictate the node to be loaded more often since it would allow a bigger error range.
        /// A lower error will prevent the node to be loaded by have a smaller error range.
        /// </summary>
        /// <remarks>An error set to <see langword="float"/>.MaxValue would force the node to always be loaded
        /// if its parent is loaded.</remarks>
        public float GeometricError { get; }

        /// <summary>
        /// Determine how to handle the node when it get <see cref="TargetState.Expanded"/>
        /// </summary>
        public RefinementMode RefinementMode { get; }


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="content">Fill the <see cref="NodeData"/> with this content values.</param>
        /// <param name="refinementMode">Refinement mode to be applied when expanded.</param>
        public NodeData(NodeContent content, RefinementMode refinementMode)
        {
            AlwaysExpand = content.AlwaysExpand;
            Bounds = content.Bounds;
            GeometricError = content.GeometricError;
            RefinementMode = refinementMode;
        }

        /// <summary>
        /// Constructor with explicit values for each properties.
        /// </summary>
        /// <param name="bounds">Limits of the node geometry allowing to calculate the intersection between the node geometry
        /// with the <see cref="UGSceneObserver"/> bounds.</param>
        /// <param name="geometricError">Maximum allowed geometric error indicating if the node should be loaded.
        /// Usually correspond to the screen space the node should have before being loaded.
        /// A high error will dictate the node to be loaded more often since it would allow a bigger error range.
        /// A lower error will prevent the node to be loaded by have a smaller error range.
        /// <remarks>An error set to <see langword="float"/>.MaxValue would force the node to always be loaded
        /// if its parent is loaded.</remarks></param>
        /// <param name="refinementMode">Determine how to handle the node when it get <see cref="TargetState.Expanded"/>.</param>
        /// <param name="alwaysExpand">If the node should always be expanded without calculating the <see cref="GeometricError"/>;
        /// <see langword="false"/> if the <see cref="GeometricError"/> should always be calculated when
        /// <see cref="TargetState.Expanded"/> state needs to be evaluated.</param>
        public NodeData(DoubleBounds bounds, float geometricError, RefinementMode refinementMode = RefinementMode.Add, bool alwaysExpand = false)
        {
            AlwaysExpand = alwaysExpand;
            Bounds = bounds;
            GeometricError = geometricError;
            RefinementMode = refinementMode;
        }

        /// <summary>
        /// <see cref="NodeData"/> to be used the node needs to be always expanded.
        /// </summary>
        public static readonly NodeData Placeholder = new NodeData
        (
            new DoubleBounds(double3.zero, new double3(double.MaxValue, double.MaxValue, double.MaxValue)),
            float.MaxValue,
            RefinementMode.Add,
            true
        );
    }
}
