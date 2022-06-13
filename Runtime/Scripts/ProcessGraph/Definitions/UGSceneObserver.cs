using System.Collections.Generic;

namespace Unity.Geospatial.Streaming
{

    public abstract class UGSceneObserver : UGProcessingNode
    {
        protected UGSceneObserver(int detailOutputCount)
        {
        }

        protected readonly List<NodeOutput<DetailObserverData>> m_DetailOutputs = new List<NodeOutput<DetailObserverData>>();

        public int DetailOutputCount
        {
            get { return m_DetailOutputs.Count; }
        }

        public NodeOutput<DetailObserverData> GetDetailOutput(int index)
        {
            return m_DetailOutputs[index];
        }
    }

}
