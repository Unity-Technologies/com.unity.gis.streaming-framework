using System;

using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The Simple Scene Observer is a Unity workflow agnostic scene observer which
    /// is completed by a Unity workflow dependant implementation. For example, the 
    /// <see name="UGCameraBehaviour"/> completes this class' implementation by converting the
    /// Unity Camera and Unity Transform properties and members into the abstracted
    /// <see name="DetailObserverData"/> struct.
    /// </summary>
    public class UGSimpleSceneObserver : UGSceneObserver
    {
        /// <summary>
        /// Allow to convert an <see cref="UGSceneObserver"/> to a <see cref="DetailObserverData"/> instance.
        /// </summary>
        public interface IImplementation
        {
            /// <summary>
            /// Method used to convert the <see cref="UGSimpleSceneObserver"/> instance to a <see cref="DetailObserverData"/>.
            /// </summary>
            /// <returns>The necessary data to calculate the geometric error of a geometry.</returns>
            DetailObserverData GetDetailObserverData();
        }
        
        private readonly IImplementation m_Implementation;

        private readonly NodeOutput<DetailObserverData> m_DetailOutput;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="implementation">
        /// Allow to convert an <see cref="UGSceneObserver"/> to a <see cref="DetailObserverData"/> instance.
        /// </param>
        public UGSimpleSceneObserver(IImplementation implementation)
        {
            Assert.IsNotNull(implementation);
            m_Implementation = implementation;

            m_DetailOutput = new NodeOutput<DetailObserverData>(this);
            m_DetailOutputs.Add(m_DetailOutput);
        }

        /// <summary>
        /// Implementation of <see cref="UGProcessingNode.ScheduleMainThread"/>.
        /// </summary>
        public override bool ScheduleMainThread
        {
            get { return false; }
        }

        /// <summary>
        /// Implementation of <see cref="UGProcessingNode.IsProcessing"/>.
        /// </summary>
        protected override bool IsProcessing
        {
            get { return false; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            //
            //  Method left intentionally blank
            //
        }

        /// <summary>
        /// Implementation of <see cref="UGProcessingNode.MainThreadProcess"/>.
        /// </summary>
        public override void MainThreadProcess()
        {
            throw new NotImplementedException(
                "This method should never be called because ScheduleMainThread is always false");
        }

        /// <summary>
        /// Implementation of <see cref="UGProcessingNode.MainThreadUpKeep"/>.
        /// </summary>
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
