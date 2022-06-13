using System;

using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The Simple Scene Observer is a Unity workflow agnostic scene observer which
    /// is completed by a Unity workflow dependant implementation. For example, the 
    /// UGCameraBehaviour completes this class' implementation by converting the
    /// Unity Camera and Unity Transform properties and members into the abstracted
    /// DetailObserverData struct.
    /// </summary>
    public class UGSimpleSceneObserver : UGSceneObserver
    {
        public interface IImplementation
        {
            DetailObserverData GetDetailObserverData();
        }
        
        private readonly IImplementation m_Implementation;

        private readonly NodeOutput<DetailObserverData> m_DetailOutput;


        public UGSimpleSceneObserver(IImplementation implementation) : base(1)
        {
            Assert.IsNotNull(implementation);
            m_Implementation = implementation;

            m_DetailOutput = new NodeOutput<DetailObserverData>(this);
            m_DetailOutputs.Add(m_DetailOutput);
        }

        public override bool ScheduleMainThread
        {
            get { return false; }
        }

        protected override bool IsProcessing
        {
            get { return false; }
        }


        public override void Dispose()
        {
            //
            //  Method left intentionally blank
            //
        }

        public override void MainThreadProcess()
        {
            throw new NotImplementedException("This method should never be called because ScheduleMainThread is always false");
        }

        public override void MainThreadUpKeep()
        {
            Assert.IsNotNull(m_Implementation);

            if (!m_DetailOutputs[0].IsReadyForData)
                return;
            
            DetailObserverData observerData = m_Implementation.GetDetailObserverData();

            m_DetailOutput.ProcessData(ref observerData);
        }
    }

}
