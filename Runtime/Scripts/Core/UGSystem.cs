using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine.Assertions;
using UnityEngine.Profiling;

using Debug = UnityEngine.Debug;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Serves as the center piece to the Unity Geospatial Framework.
    /// Responsible to build <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see> type objects
    /// (in contrast with DOTS type objects)
    /// 
    /// During runtime, this class will instantiate a number of other
    /// <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see> which will serve to render
    /// the environment as it is streamed in from various sources.
    /// </summary>
    public class UGSystem : IDisposable
    {
        /// <summary>
        /// Event called when a <see cref="UGProcessingNode"/> starts to be processed and none where previously.
        /// </summary>
        public event Action<SystemProcessingEventArgs> OnBeginProcessing;

        /// <summary>
        /// Event called when all <see cref="UGProcessingNode"/> got executed and no more are pending to be executed.
        /// </summary>
        public event Action<SystemProcessingEventArgs> OnEndProcessing;

        /// <summary>
        /// <see cref="UGSystem"/> configuration used when creating a new <see cref="UGSystem"/> instance.
        /// Allows to create <see cref="UGSystem"/> on the fly and also makes the serialization easier.
        /// </summary>
        public struct Configuration
        {
            /// <summary>
            /// Scene observers determine which parts of the universe are streamed into the scene. Examples of scene observers
            /// are cameras, bounding boxes, bounding spheres, etc.
            /// </summary>
            public UGSceneObserver[] SceneObservers;
            
            /// <summary>
            /// This is the list of Geospatial layers that will be streamed. See <see cref="UGDataSource"/> for details
            /// as to the layer's configuration.
            /// </summary>
            public UGDataSource[] DataSources;
            
            /// <summary>
            /// This is the modifier stack. Modifiers are applied to the streamed data in the order they are set in
            /// this list, from 0 to last.
            /// </summary>
            public UGModifier[] Modifiers;
            
            /// <summary>
            /// These are the presenters which will convert the streamed geometry into actual
            /// <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObjects</see>. Most
            /// configurations will only require a single presenter but applications with multiple cameras or applications
            /// where the source data is not normalized in space (think multiple planets or non-geolocated dataset) may
            /// require multiple presenters.
            /// </summary>
            public UGPresenter[] Presenters;

            /// <summary>
            /// The material factory allows the UG System to take function with any render pipeline, provided that
            /// the appropriate material factory has been created and has been assigned.
            /// 
            /// The Unity Geospatial Framework comes with material factories for both the Built-in render pipeline
            /// as well as the Universal Render Pipeline.
            /// </summary>
            public UGMaterialFactory MaterialFactory;

            /// <summary>
            /// This is the time, in milliseconds, that the UG System is allowed to use on the main thread. As
            /// much as possible, the Geospatial Framework tries to offload as much of the CPU intensive tasks
            /// to threads populated onto the job system. However, some action need to be performed by the main
            /// thread such as creating GameObjects and some Unity render objects. 
            /// 
            /// These main thread actions are queued up by the other threads, each frame, a time-slot of the
            /// specified duration is allocated to perform them. If the time used by the main thread exceeds
            /// the specified duration, excess actions will be postponed to the next frame.
            /// 
            /// It is normal and expected that this time limit be exceeded. It defines a cut-off as to when
            /// an action can be started. There is no predictive logic as to how long a task might take. Therefore
            /// if the last task was started at 9ms and took 8ms, the allocated time would expand to 17ms.
            /// 
            /// Setting this parameter to 0 will cause only a single action to be performed each frame.
            /// </summary>
            public float MainThreadTimeLimitMs;
            
            /// <summary>
            /// This is the amount of files that will be loaded simultaneously.
            /// If the limit has been reached, items part of the queue needs to be completed before new ones can be added.
            /// </summary>
            /// <remarks>
            /// The higher the value, faster execution time you will get. But if the value is too high, slower
            /// it will get since the system will not be able to adapt when <see cref="UGSceneObserver"/> are in movement.
            /// If the value is too low, it will take more time to get a higher resolution, but you will gain reaction speed.
            /// </remarks>
            public int MaximumSimultaneousContentRequests;
            
            /// <summary>
            /// The streaming mode is an advanced feature that allows the <see cref="UGSystem"/> to trade off
            /// impact on the simulation's framerate in exchange for faster streaming.
            /// </summary>
            public StreamingModes StreamingMode;

            /// <summary>
            /// Allow the <see cref="UGSystem"/> to run tasks on multiple threads. Leave this on for most platforms but
            /// turn off for platforms that do not support threading such as WebGL.
            /// </summary>
            public bool AllowMultithreading;

            /// <summary>
            /// Construct a new <see cref="Configuration"/> instance with the default values.
            /// </summary>
            public static Configuration Default
            {
                get
                {
                    Configuration result;

                    result.SceneObservers = null;
                    result.DataSources = null;
                    result.Modifiers = null;
                    result.Presenters = null;

                    result.MaterialFactory = null;

                    result.MainThreadTimeLimitMs = 10;
                    result.MaximumSimultaneousContentRequests = 10;
                    result.StreamingMode = StreamingModes.MinimalImpact;

                    result.AllowMultithreading = false;

                    return result;
                }
            }
        }
        
        private struct DataSourceConfig
        {
            public Type DecoderType;
            public Func<UUIDGenerator, UGMaterialFactory, UGDataSource[], int, UGDataSourceDecoder> Instantiator;
            public List<UGDataSource> DataSources;
            public int MaximumSimultaneousContentRequests;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="configuration">Apply the values from this configuration to the newly created <see cref="UGSystem"/>.</param>
        /// <exception cref="InvalidOperationException">Raised where the given <paramref name="configuration"/>
        /// has invalid values.</exception>
        public UGSystem(ref Configuration configuration)
        {
            if (configuration.SceneObservers.Length < 1)
                throw new InvalidOperationException("UGSystem expects one or more scene observers");

            if (configuration.SceneObservers.Contains(null))
                throw new InvalidOperationException("Null observer provided to UGSystem");


            if (configuration.DataSources.Length < 1)
                throw new InvalidOperationException("UGSystem expects at least one data source");

            if (configuration.DataSources.Contains(null))
                throw new InvalidOperationException("Null data source provided to UGSystem");

            if (configuration.Modifiers.Contains(null))
                throw new InvalidOperationException("Null modifier provided to UGSystem");

            if (configuration.Presenters.Length < 1)
                throw new InvalidOperationException("UGSystem requires at least one presenter.");

            if (configuration.Presenters.Contains(null))
                throw new InvalidOperationException("Null presenter provided to UGSystem");

            if (configuration.MaterialFactory == null)
                throw new InvalidOperationException("UGSystem requires a material factory to be specified");

            if (configuration.MaximumSimultaneousContentRequests < 1)
                throw new InvalidOperationException("UGSystem requires a positive maximum simultaneous loads value");

            UUIDGenerator idGenerator = UUIDGenerator.Instance;
            UGMaterialFactory materialFactory = configuration.MaterialFactory;
            m_MainThreadTimeLimitMs = configuration.MainThreadTimeLimitMs;
            m_StreamingMode = configuration.StreamingMode;

            if (configuration.AllowMultithreading)
                m_TaskManager = new ThreadPoolTaskManager();
            else
                m_TaskManager = new MainThreadTaskManager();

            //
            //  Connect Presenters
            //
            m_Presenters = configuration.Presenters;
            m_ProcessingNodes.AddRange(m_Presenters);

            //
            //  Connect Instantiator
            //
            UGInstantiator instantiator = new UGInstantiator(m_Presenters.Length);
            for (int i = 0; i < m_Presenters.Length; i++)
                UGProcessingNode.Connect(instantiator.GetOutput(i), m_Presenters[i].Input);
            m_ProcessingNodes.Add(instantiator);

            //
            //  Connect modifiers
            //
            
            int modifierCount = configuration.Modifiers.Length;
            if (modifierCount > 0)
            {
                for (int i = 0; i < modifierCount - 1; i++)
                    UGProcessingNode.Connect(configuration.Modifiers[i].Output, configuration.Modifiers[i + 1].Input);
                UGProcessingNode.Connect(configuration.Modifiers[modifierCount - 1].Output, instantiator.Input);
                m_ProcessingNodes.AddRange(configuration.Modifiers);
            }

            //
            //  Connect decoders
            //
            List<DataSourceConfig> dataSourceConfig = new List<DataSourceConfig>();
            foreach (UGDataSource dataSource in configuration.DataSources)
            {
                if (dataSource == null)
                    continue;

                Type decoderType = dataSource.GetDecoderType();

                DataSourceConfig config = dataSourceConfig
                    .SingleOrDefault(c => c.DecoderType == decoderType);

                if (config.DecoderType == null)
                {
                    config.DecoderType = decoderType;
                    config.Instantiator = dataSource.InstantiateDecoder;
                    config.DataSources = new List<UGDataSource>();
                    config.DataSources.Add(dataSource);
                    dataSourceConfig.Add(config);
                    config.MaximumSimultaneousContentRequests = configuration.MaximumSimultaneousContentRequests;
                }
                else
                {
                    config.DataSources.Add(dataSource);
                }
            }

            int totalDecoderOutputCount = 0;
            List<UGDataSourceDecoder> decoders = new List<UGDataSourceDecoder>(dataSourceConfig.Count);
            foreach (DataSourceConfig config in dataSourceConfig)
            {
                UGDataSourceDecoder decoder = config.Instantiator.Invoke(
                    idGenerator,
                    materialFactory,
                    config.DataSources.ToArray(),
                    configuration.MaximumSimultaneousContentRequests);

                totalDecoderOutputCount += decoder.OutputCount;
                decoders.Add(decoder);
            }

            DecoderMultiplexer multiplexer = new DecoderMultiplexer(totalDecoderOutputCount);
            int multiplexerInputIndex = 0;
            foreach (UGDataSourceDecoder decoder in decoders)
            {
                for (int i = 0; i < decoder.OutputCount; i++)
                    UGProcessingNode.Connect(decoder.GetOutput(i), multiplexer.GetInput(multiplexerInputIndex++));
            }

            if (modifierCount > 0)
                UGProcessingNode.Connect(multiplexer.Output, configuration.Modifiers[0].Input);
            else
                UGProcessingNode.Connect(multiplexer.Output, instantiator.Input);
            m_ProcessingNodes.AddRange(decoders);


            //
            //  Connect Observers
            //
            int observerOutputCount = configuration.SceneObservers.Sum(o => o.DetailOutputCount);
            ObserverMultiplexer observerMultiplexer = new ObserverMultiplexer(observerOutputCount);
            int observerMultiplexerIndex = 0;
            foreach (UGSceneObserver observer in configuration.SceneObservers)
            {
                for (int i = 0; i < observer.DetailOutputCount; i++)
                {
                    UGProcessingNode.Connect(observer.GetDetailOutput(i), observerMultiplexer.GetInput(observerMultiplexerIndex++));
                }
            }

            OneToManyNode<DetailObserverData[]> observerOneToMany = new OneToManyNode<DetailObserverData[]>(decoders.Count);
            UGProcessingNode.Connect(observerMultiplexer.Output, observerOneToMany.Input);
            for (int i = 0; i < decoders.Count; ++i)
            {
                UGProcessingNode.Connect(observerOneToMany.GetOutput(i), decoders[i].Input);
            }

            m_ProcessingNodes.Add(observerMultiplexer);
            m_ProcessingNodes.Add(observerOneToMany);
            m_ProcessingNodes.AddRange(configuration.SceneObservers);

            //
            //  Register with task manager
            //
            foreach (UGProcessingNode node in m_ProcessingNodes)
                node.SetTaskManager(m_TaskManager);
           
        }
        
        /// <summary>
        /// The streaming mode is an advanced feature that allows the <see cref="UGSystem"/> to trade off
        /// impact on the simulation's framerate in exchange for faster streaming.
        /// </summary>
        public enum StreamingModes
        {
            /// <summary>
            /// The frame rate is the priority. When processing, if a <see cref="UGCommandBuffer">command</see>
            /// is taking too much time, it's completion will be delayed to the next frame.
            /// </summary>
            MinimalImpact,
            
            /// <summary>
            /// The simulation is the priority. <see cref="UGCommandBuffer">Commands</see> must be completed before
            /// moving to the next frame.
            /// </summary>
            HoldFrame
        }

        private const int k_OutOfTimeFrameCounterWarningLimit = 5;

        private readonly IExecutableTaskManager m_TaskManager;

        private readonly UGPresenter[] m_Presenters;

        /// <summary>
        /// Number of frames to wait before triggering the <see cref="OnEndProcessing"/> event.
        /// </summary>
        private const int k_ProcessingEventTimeout = 10;

        /// <summary>
        /// Used to store the amount of frames has been waited before triggering <see cref="OnEndProcessing"/>.
        /// </summary>
        private int m_ProcessingEventTimer = k_ProcessingEventTimeout + 1;

        private bool m_UncaughtExceptionHasOccured;
        
        /// <summary>
        /// <see langword="true"/> if the instance was disposed and cannot be used anymore;
        /// <see langword="false"/> otherwise.
        /// </summary>
        private bool m_Disposed;

        private readonly float m_MainThreadTimeLimitMs;
        private readonly StreamingModes m_StreamingMode;
        private readonly Stopwatch m_Stopwatch = new Stopwatch();

        /// <summary>
        /// List of nodes that needs to be evaluated by the Process Graph on each frame.
        /// </summary>
        private readonly List<UGProcessingNode> m_ProcessingNodes = new List<UGProcessingNode>();
        private int m_OutOfTimeFrameCounter;

        private readonly UGSynchronizationContext m_SynchronizationContext = new UGSynchronizationContext();

        /// <summary>
        /// Destructor called by the garbage collector.
        /// </summary>
        ~UGSystem()
        {
            Assert.IsTrue(m_Disposed, "UG Client has not been disposed, this will result in undefined behaviour");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// This will make sure all running main threads are completed and processing nodes are disposed.
        /// </summary>
        public void Dispose()
        {
            m_SynchronizationContext.SetContext();
            while (m_SynchronizationContext.ScheduleMainThread || m_TaskManager.MainThreadTasks > 0 || m_TaskManager.ThreadPoolTasks > 0)
            {
                m_SynchronizationContext.MainThreadProcess();
                m_TaskManager.ExecuteMainThreadTask();
            }
            m_SynchronizationContext.ResetContext();

            m_Disposed = true;
            m_ProcessingNodes.ForEach(node => node.Dispose());
        }

        private bool PresentersAreIdle()
        {
            foreach (UGPresenter presenter in m_Presenters)
            {
                if (presenter.DataAvailabilityStatus != UGProcessingNode.DataAvailability.Idle)
                    return false;
            }
            return true;
        }

        /// <returns>
        /// <see langword="true"/> if all <see cref="UGProcessingNode"/> part of this system have been executed;
        /// <see langword="false"/> otherwise.
        /// </returns>
        private bool IsIdle()
        {
            foreach (UGProcessingNode node in m_ProcessingNodes)
            {
                if (!node.IsIdle)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Nodes to be evaluated by the Process Graph on each frame.
        /// </summary>
        public IEnumerable<UGProcessingNode> ProcessingNodes
        {
            get
            {
                return m_ProcessingNodes.AsEnumerable();
            }
        }

        /// <summary>
        /// The streaming mode is an advanced feature that allows the <see cref="UGSystem"/> to trade off
        /// impact on the simulation's framerate in exchange for faster streaming.
        /// </summary>
        public StreamingModes StreamingMode
        {
            get
            {
                return m_StreamingMode;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetTimer()
        {
            m_Stopwatch.Restart();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TimerHasExpired()
        {
            return m_Stopwatch.ElapsedMilliseconds >= m_MainThreadTimeLimitMs;
        }

        private void TimeLimitedProcess()
        {
            //
            //  Loop until
            //      - Presenters are idle
            //      - Time has elapsed (only if minimal impact loading)
            //      - All nodes don't need main thread time (only if minimal impact loading)
            //

            bool minimalImpactLoading = (m_StreamingMode == StreamingModes.MinimalImpact);

            bool didWork;
            do
            {
                didWork = false;

                while(m_TaskManager.MainThreadTasks > 0)
                {
                    m_TaskManager.ExecuteMainThreadTask();

                    didWork = true;

                    if (TimerHasExpired())
                        return;
                }

                while (m_SynchronizationContext.ScheduleMainThread)
                {
                    m_SynchronizationContext.MainThreadProcess();

                    didWork = true;

                    if (TimerHasExpired())
                        return;
                }

                foreach (UGProcessingNode node in m_ProcessingNodes)
                {
                    while (node.ScheduleMainThread)
                    {
                        node.MainThreadProcess();

                        didWork = true;

                        if (TimerHasExpired())
                            return;
                    }
                }

                if (minimalImpactLoading && !didWork)
                    return;

            } while (didWork || !PresentersAreIdle());
        }

        /// <summary>
        /// Call the <see cref="OnBeginProcessing"/> if new nodes will be loaded and the system was idle at the previous frame.
        /// If all nodes are loaded and no new nodes needs to be loaded, the <see cref="OnEndProcessing"/> will be called.
        /// </summary>
        private void HandleProcessingEvents()
        {
            if (IsIdle())
            {
                if (m_ProcessingEventTimer >= k_ProcessingEventTimeout)
                    return;

                m_ProcessingEventTimer++;

                if (m_ProcessingEventTimer == k_ProcessingEventTimeout)
                {
                    OnEndProcessing?.Invoke(new SystemProcessingEventArgs());
                    m_ProcessingEventTimer++;
                }

                return;
            }

            if (m_ProcessingEventTimer > k_ProcessingEventTimeout)
                OnBeginProcessing?.Invoke(new SystemProcessingEventArgs());

            m_ProcessingEventTimer = 0;
        }

        /// <summary>
        /// Evaluate each <see cref="UGProcessingNode"/> part of <see cref="UGSystem.ProcessingNodes"/>.
        /// </summary>
        public void ProcessFrame()
        {
            if (m_UncaughtExceptionHasOccured)
                return;

            ResetTimer();

            m_SynchronizationContext.SetContext();

            try
            {
                Profiler.BeginSample("Main Thread Upkeep");
                foreach (UGProcessingNode node in m_ProcessingNodes)
                    node.MainThreadUpKeep();
                Profiler.EndSample();

                if (TimerHasExpired())
                    m_OutOfTimeFrameCounter++;
                else
                    m_OutOfTimeFrameCounter = 0;

                if (m_OutOfTimeFrameCounter > k_OutOfTimeFrameCounterWarningLimit)
                    Debug.LogWarning("Geospatial upkeep has exceeded the allotted processing time. Please increase the UGSystem's Main Thread Time Limit");


                Profiler.BeginSample(nameof(TimeLimitedProcess));
                TimeLimitedProcess();
                Profiler.EndSample();
            }
            catch(Exception e)
            {
                m_UncaughtExceptionHasOccured = true;
                Debug.LogException(e);
            }

            m_SynchronizationContext.ResetContext();

            HandleProcessingEvents();
        }
    }
}
