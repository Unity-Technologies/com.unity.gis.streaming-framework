using System.Collections.Generic;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Base Scene Observer class used to calculate the <see name="DetailObserverData"/> values.
    /// </summary>
    public abstract class UGSceneObserver : UGProcessingNode
    {
        /// <summary>
        /// List of <see cref="UGProcessingNode.NodeOutput{T}"/> returning each <see cref="DetailObserverData"/> for each
        /// registered observer.
        /// </summary>
        protected readonly List<NodeOutput<DetailObserverData>> m_DetailOutputs = new List<NodeOutput<DetailObserverData>>();

        /// <summary>
        /// Get the number of <see cref="UGProcessingNode.NodeOutput{T}"/> have been created with a <see cref="DetailObserverData"/>.
        /// </summary>
        public int DetailOutputCount
        {
            get { return m_DetailOutputs.Count; }
        }

        /// <summary>
        /// Get the <see cref="DetailObserverData"/> <see cref="UGProcessingNode.NodeOutput{T}"/> for the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of the instance to retrieve.</param>
        /// <returns>The node output instance at the given <paramref name="index"/>.</returns>
        public NodeOutput<DetailObserverData> GetDetailOutput(int index)
        {
            return m_DetailOutputs[index];
        }
    }

}
