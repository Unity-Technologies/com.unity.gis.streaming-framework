using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Runtime.CompilerServices;

using UnityEngine.Assertions;
using UnityEngine.Profiling;

using Debug = UnityEngine.Debug;

namespace Unity.Geospatial.Streaming
{
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

        public struct Configuration
        {
            public UGSceneObserver[] SceneObservers;
            public UGDataSource[] DataSources;
            public UGModifier[] Modifiers;
            public UGPresenter[] Presenters;

            public UGMaterialFactory MaterialFactory;

            public float MainThreadTimeLimitMs;
            public int MaximumSimultaneousContentRequests;
            public StreamingModes StreamingMode;

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

                    return result;
                }
            }
        }

        private class TaskManager : ITaskManager
        {
            private readonly object m_Lock = new object();

            public int UnfinishedTaskCount { get; private set; }

            //
            //  TODO - Consider changing this to a signature closer to that of the Thread Pool
            //
            public void ScheduleTask(Action task)
            {
                lock (m_Lock)
                {
                    UnfinishedTaskCount++;
                }

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    task.Invoke();
                    lock (m_Lock)
                    {
                        UnfinishedTaskCount--;
                    }
                });
            }
        }

        private struct DataSourceConfig
        {
            public Type DecoderType;
            public Func<UUIDGenerator, UGMaterialFactory, UGDataSource[], int, UGDataSourceDecoder> Instantiator;
            public List<UGDataSource> DataSources;
            public int MaximumSimultaneousContentRequests;
        }

        

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

        public enum StreamingModes
        {
            MinimalImpact,
            HoldFrame
        }

        private const int k_OutOfTimeFrameCounterWarningLimit = 5;

        private readonly TaskManager m_TaskManager = new TaskManager();
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
        private bool m_Disposed;

        private readonly float m_MainThreadTimeLimitMs;
        private readonly StreamingModes m_StreamingMode;
        private readonly Stopwatch m_Stopwatch = new Stopwatch();

        private readonly List<UGProcessingNode> m_ProcessingNodes = new List<UGProcessingNode>();
        private int m_OutOfTimeFrameCounter;

        private readonly UGSynchronizationContext m_SynchronizationContext = new UGSynchronizationContext();

        ~UGSystem()
        {
            Assert.IsTrue(m_Disposed, "UG Client has not been disposed, this will result in undefined behaviour");
        }

        public void Dispose()
        {
            m_SynchronizationContext.SetContext();
            while (m_SynchronizationContext.ScheduleMainThread || m_TaskManager.UnfinishedTaskCount > 0)
            {
                m_SynchronizationContext.MainThreadProcess();
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

        public IEnumerable<UGProcessingNode> ProcessingNodes
        {
            get
            {
                return m_ProcessingNodes.AsEnumerable();
            }
        }

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
