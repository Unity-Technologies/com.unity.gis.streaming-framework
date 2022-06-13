using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    public class UGUniversalDecoder : UGDataSourceDecoder
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="materialFactory">Which material factory to be used by the renderer.</param>
        /// <param name="dataSources">Array of <see cref="UGDataSource"/> instance available to be loaded.</param>
        /// <param name="maximumSimultaneousContentRequests">The <see cref="ExpansionScheduler.MaximumSimultaneousContentRequests"/> will be set to this value.</param>
        public UGUniversalDecoder(UGMaterialFactory materialFactory, IEnumerable<UGDataSource> dataSources, int maximumSimultaneousContentRequests) :
            base(outputCount: 1)
        {
            m_Output = GetOutput(0);

            UGCommandBuffer commandBuffer = new UGCommandBuffer(UUIDGenerator.Instance);

            m_CommandBufferProcessor = new UGCommandBufferProcessor(materialFactory, commandBuffer, m_Output);

            m_TargetStateController = new TargetStateController(m_BoundingVolumeHierarchy);
            m_ContentManager = new NodeContentManager(m_BoundingVolumeHierarchy, commandBuffer, TaskManager);
            CurrentStateController currentStateController = new CurrentStateController(m_BoundingVolumeHierarchy, m_ContentManager);
            m_ExpansionScheduler = new ExpansionScheduler(m_BoundingVolumeHierarchy, currentStateController);

            m_ExpansionScheduler.MaximumSimultaneousContentRequests = maximumSimultaneousContentRequests;

            foreach (UGDataSource dataSource in dataSources)
                AddDataSource(dataSource);
        }


        /// <summary>
        /// Registered <see cref="DetailObserverData"/> instances used to calculate the lowest screen space error.
        /// </summary>
        private DetailObserverData[] m_DetailObserverData = new DetailObserverData[1];

        /// <summary>
        /// The target state controller class which controls the target state of the BVH.
        /// </summary>
        private readonly TargetStateController m_TargetStateController;

        /// <summary>
        /// BVH used to store the <see cref="NodeId"/> <see cref="NodeContent"/>
        /// and the associated <see cref="CurrentState"/> and <see cref="TargetState"/>.
        /// </summary>
        private readonly BoundingVolumeHierarchy<ExpansionScheduler.Cache> m_BoundingVolumeHierarchy = new BoundingVolumeHierarchy<ExpansionScheduler.Cache>();

        /// <summary>
        /// The expansion scheduler, which interacts with the <see cref="CurrentStateController"/> to determine which
        /// nodes should be expanded first.
        /// </summary>
        private readonly ExpansionScheduler m_ExpansionScheduler;

        /// <summary>
        /// Responsible to execute directives by matching the
        /// <see cref="BoundingVolumeHierarchy{T}.GetCurrentState">Node State</see> with its corresponding
        /// <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>.
        /// </summary>
        private readonly NodeContentManager m_ContentManager;

        /// <summary>
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Tasks</see> to be executed.
        /// </summary>
        private readonly List<TaskCompletionSource<object>> m_StartNextFrame = new List<TaskCompletionSource<object>>();

        /// <summary>
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1?view=net-6.0">Queue</see>
        /// of <see cref="UGCommandBuffer.Command"/> to be executed.
        /// </summary>
        private readonly UGCommandBufferProcessor m_CommandBufferProcessor;

        /// <summary>
        /// Node graph output of the main thread command.
        /// </summary>
        private readonly NodeOutput<InstanceCommand> m_Output;

        /// <summary>
        /// Get the <see cref="BoundingVolumeHierarchy{T}"/> update task allowing to know if completed.
        /// </summary>
        private Task m_BvhUpdateTask;

        /// <summary>
        /// This is the amount of files that will be loaded simultaneously.
        /// If the limit has been reached, items part of the queue needs to be completed before new ones can be added.
        /// </summary>
        /// <remarks>
        /// The higher the value, faster execution time you will get. But if the value is too high, slower
        /// it will get since the system will not be able to adapt when <see cref="UGSceneObserver"/> are in movement.
        /// If the value is too low, it will take more time to get a higher resolution, but you will gain reaction speed.
        /// </remarks>
        public int MaximumSimultaneousContentRequests
        {
            get
            {
                return m_ExpansionScheduler.MaximumSimultaneousContentRequests;
            }
        }

        /// <summary>
        /// <see langword="true"/> if the decoder is currently processing.;
        /// <see langword="false"/> otherwise.
        /// </summary>
        public override bool ScheduleMainThread
        {
            get
            {
                //
                //  If the output can't accept data, leave CPU time
                //  for down-stream tasks
                //
                if (!m_Output.IsReadyForData)
                    return false;

                //
                //  If any of our internal components need CPU time,
                //  then return true.
                //
                return  !m_CommandBufferProcessor.IsComplete() ||
                        m_ContentManager.GetState() == NodeContentManager.State.Processing ||
                        m_ExpansionScheduler.GetState() == ExpansionScheduler.State.Processing;
            }
        }

        /// <summary>
        /// <see langword="true"/> if the <see cref="NodeContentManager"/> is currently processing;
        /// <see langword="false"/> if in idle state.
        /// </summary>
        protected override bool IsProcessing
        {
            get { return m_ExpansionScheduler.GetState() != ExpansionScheduler.State.Done; }
        }

        /// <summary>
        /// Register the given <see cref="UGDataSource"/> to be used when the <see cref="m_BoundingVolumeHierarchy"/> gets updated.
        /// </summary>
        /// <param name="dataSource">Data source to register.</param>
        /// <exception cref="InvalidOperationException">If no decoder is registered for this kind of data.</exception>
        private void AddDataSource(UGDataSource dataSource)
        {
            UniversalDecoderDataSource universalDataSource = dataSource as UniversalDecoderDataSource;

            Assert.IsNotNull(universalDataSource, "Invalid data source provided to universal decoder");

            universalDataSource.InitializerDecoder(m_ContentManager);
        }

        /// <summary>
        /// Releasing unmanaged resources.
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose"/>.
        /// </summary>
        public override void Dispose()
        {
            //
            //  Intentionally left blank for now
            //
        }

        /// <summary>
        /// Request an update of the <see cref="BoundingVolumeHierarchy{T}"/> only if it's available.
        /// </summary>
        public override void MainThreadUpKeep()
        {
            StartNextFrameTasks();

            if (m_BvhUpdateTask is { IsCompleted: false })
                return;

            m_BvhUpdateTask = UpdateBvh();
        }

        /// <summary>
        /// Execute a full round of update by getting the new expected <see cref="TargetState"/>,
        /// unloading all no more required <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObjects</see>
        /// and load the newly required <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObjects</see>.
        /// </summary>
        public async Task UpdateBvh()
        {
            try
            {
                await WaitNextFrame();
                m_TargetStateController.UpdateTargetState(m_DetailObserverData);

                await WaitNextFrame();

                //
                // Need to unload first since the loading process could prevent visibility / unloading calls since it
                // when it reached the simultaneous loading limit.
                //
                await WaitNextFrame();
                m_ExpansionScheduler.Reset();

                await WaitNextFrame();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Wait for the next frame to be completed
        /// </summary>
        /// <returns>The <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Task</see>
        /// registered to be executed.</returns>
        private Task WaitNextFrame()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            m_StartNextFrame.Add(tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Prepare the task list for a new frame evaluation by clearing the <see cref="m_StartNextFrame"/> content.
        /// </summary>
        private void StartNextFrameTasks()
        {
            foreach (TaskCompletionSource<object> tcs in m_StartNextFrame)
            {
                tcs.SetResult(null);
            }
            m_StartNextFrame.Clear();
        }

        /// <summary>
        /// Execute the next pending command part of the <see cref="UGCommandBufferProcessor"/>.
        /// </summary>
        public override void MainThreadProcess()
        {
            if (!m_CommandBufferProcessor.IsComplete())
            {
                m_CommandBufferProcessor.TryExecuteSingle();
            }
            else if (m_ContentManager.GetState() == NodeContentManager.State.Processing)
            {
                m_ContentManager.ProcessNext();
            }
            else
            {
                Assert.AreEqual(ExpansionScheduler.State.Processing, m_ExpansionScheduler.GetState());
                m_ExpansionScheduler.ProcessNext(Time.timeAsDouble);
            }
        }

        /// <summary>
        /// Change the <see cref="DetailObserverData"/> to evaluate on the next frame.
        /// </summary>
        /// <param name="data">Set the observers to this <see langword="Array"/>.</param>
        public override void SetDetailObserverData(DetailObserverData[] data)
        {
            m_DetailObserverData = data;
        }
    }

}
