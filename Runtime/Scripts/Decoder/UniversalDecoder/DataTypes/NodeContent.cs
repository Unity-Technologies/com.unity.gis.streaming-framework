using UnityEngine.Assertions;
using Unity.Geospatial.HighPrecision;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Base class used to load a node data (geometry, texture, point cloud...).
    /// </summary>
    public abstract class NodeContent
    {
        /// <summary>
        /// The underlying value of the node id. This is not readonly as it will
        /// be set by the BVH when the node content is added to a node which is
        /// subsequently added to the BVH.
        /// </summary>
        private NodeId m_NodeId;        

        /// <summary>
        /// Type of data to load.
        /// </summary>
        /// <remarks>This value should be part of <see cref="ContentType"/>.</remarks>
        public ContentType Type { get; }

        /// <summary>
        /// The <see cref="UGDataSource"/> instance id allowing to link the <see cref="NodeId"/> with it loaded content.
        /// </summary>
        /// <example>This is required by <see cref="UGExtentModifier"/> to work properly.</example>
        public UGDataSourceID DataSource { get; }

        /// <summary>
        /// The bounds of the content.
        /// </summary>
        public DoubleBounds Bounds { get; set; }

        /// <summary>
        /// The geometric error of the content.
        /// If the evaluated screen space error is higher than this value, the node will not be expanded.
        /// </summary>
        public float GeometricError { get; set; }

        /// <summary>
        /// Overrides bounds and geometric error and always expands.
        /// </summary>
        public bool AlwaysExpand { get; }

        /// <summary>
        /// The node Id of the node content. This is populated by the BVH and
        /// can only be set once.
        /// </summary>
        public NodeId NodeId
        {
            get => m_NodeId;
            set
            {
                Assert.IsTrue(m_NodeId.IsNull);
                m_NodeId = value;
            }
        }

        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="type">Type of data to load. See <see cref="ContentType"/></param>
        /// <param name="dataSource">The <see cref="UGDataSource"/> instance id this <see cref="NodeContent"/> refers to.</param>
        /// <param name="bounds">Limits of the node in space that will be evaluated within each <see cref="UGSceneObserver"/>.</param>
        /// <param name="geometricError">If the evaluated screen space error is higher than this value, the node will not be expanded.</param>
        protected NodeContent(ContentType type, UGDataSourceID dataSource, in DoubleBounds bounds, float geometricError)
        {
            m_NodeId = NodeId.Null;
            Type = type;
            DataSource = dataSource;
            Bounds = bounds;
            GeometricError = geometricError;
            AlwaysExpand = false;
        }

        /// <summary>
        /// Constructor requesting no need to evaluate the screen space error since it will be considered always to be expanded.
        /// </summary>
        /// <param name="type">Type of data to load. See <see cref="ContentType"/></param>
        /// <param name="dataSource">The <see cref="UGDataSource"/> instance id this <see cref="NodeContent"/> refers to.</param>
        protected NodeContent(ContentType type, UGDataSourceID dataSource)
        {
            m_NodeId = NodeId.Null;
            Type = type;
            DataSource = dataSource;
            Bounds = default;
            GeometricError = default;
            AlwaysExpand = true;
        }
    }
}
