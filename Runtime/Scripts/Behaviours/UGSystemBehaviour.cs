using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The UGSystem is a relatively thin Monobehaviour wrapper on the underlying UGClient
    /// class, which serves as the center piece to the Unity Geospatial Framework. It
    /// is a facade type interface which is responsible for communicating with the underlying
    /// UGSystem as well as building GameObject type objects (in contrast with DOTS
    /// type objects)
    /// 
    /// During runtime, this class will instantiate a number of other GameObjects which will
    /// serve to render the environment as it is streamed in from various sources.
    /// </summary>
    public class UGSystemBehaviour : MonoBehaviour
    {
        
        /// <summary>
        /// Allow to serialize a single <see cref="UGBehaviourPresenter"/> instance.
        /// </summary>
        [Serializable]
        public struct PresenterConfiguration
        {
            /// <summary>
            /// Index of the <see href="https://docs.unity3d.com/ScriptReference/LayerMask.html">Layer</see> this presenter is associated with.
            /// </summary>
            public int outputLayer;

            /// <summary>
            /// All the <see cref="UGDataSourceObject"/> to load.
            /// </summary>
            public int dataSources;

            /// <summary>
            /// Parent Transform of the presenter.
            /// </summary>
            public Transform outputRoot;
        }
        
        /// <inheritdoc cref="UGSystem.OnBeginProcessing"/>
        public event Action<SystemProcessingEventArgs> OnBeginProcessing;

        /// <inheritdoc cref="UGSystem.OnEndProcessing"/>
        public event Action<SystemProcessingEventArgs> OnEndProcessing;

        /// <summary>
        /// <see langword="Action"/> to be executed when ever a new <see cref="UniversalDecoder.NodeContent"/>
        /// get loaded in memory.
        /// </summary>
        /// <returns>
        ///     <typeparam>Id of the <see cref="UGBehaviourPresenter"/> part of this system triggering the event.
        ///         <name>presenter</name>
        ///     </typeparam>
        ///     <typeparam>New instance id being created.
        ///         <name>instance</name>
        ///     </typeparam>
        ///     <typeparam>Game object linked with instance.
        ///         <name>gameObject</name>
        ///     </typeparam>
        /// </returns>
        public event Action<int, InstanceID, GameObject> OnAllocateInstance;

        /// <summary>
        /// <see langword="Action"/> to be executed when ever a <see cref="UniversalDecoder.NodeContent"/>
        /// loaded in memory get disposed of.
        /// </summary>
        /// <returns>
        ///     <typeparam>Id of the <see cref="UGBehaviourPresenter"/> part of this system triggering the event.
        ///         <name>presenter</name>
        ///     </typeparam>
        ///     <typeparam>New instance id being disposed.
        ///         <name>instance</name>
        ///     </typeparam>
        ///     <typeparam>Game object linked with instance.
        ///         <name>gameObject</name>
        ///     </typeparam>
        /// </returns>
        public event Action<int, InstanceID, GameObject> OnDisposeInstance;
        
        /// <summary>
        /// <see langword="Action"/> to be executed when ever a <see cref="UniversalDecoder.NodeContent"/>
        /// visibility state changes.
        /// </summary>
        /// <returns>
        ///     <typeparam>Id of the <see cref="UGBehaviourPresenter"/> part of this system triggering the event.
        ///         <name>presenter</name>
        ///     </typeparam>
        ///     <typeparam>New instance id being updated.
        ///         <name>instance</name>
        ///     </typeparam>
        ///     <typeparam>Game object linked with instance.
        ///         <name>gameObject</name>
        ///     </typeparam>
        ///     <typeparam>
        ///         <see langword="true"/> if the instance is visible and loaded;
        ///         <see langword="false"/> otherwise.
        ///         <name>isVisible</name>
        ///     </typeparam>
        /// </returns>
        public event Action<int, InstanceID, GameObject, bool> OnUpdateInstanceVisibility;

        /// <summary>
        /// Scene observers determine which parts of the universe are streamed into the scene. Examples of scene observers
        /// are cameras, bounding boxes, bounding spheres, etc.
        /// </summary>
        public List<UGSceneObserverBehaviour> sceneObservers;

        /// <summary>
        /// This is the list of Geospatial layers that will be streamed. See <see cref="UGDataSource"/> for details
        /// as to the layer's configuration.
        /// </summary>
        public List<UGDataSourceObject> dataSources;

        /// <summary>
        /// This is the modifier stack. Modifiers are applied to the streamed data in the order they are set in
        /// this list, from 0 to last.
        /// </summary>
        public List<UGModifierBehaviour> modifiers;

        /// <summary>
        /// These are the presenters which will convert the streamed geometry into actual gameobjects. Most
        /// configurations will only require a single presenter but applications with multiple cameras or applications
        /// where the source data is not normalized in space (think multiple planets or non-geolocated dataset) may
        /// require multiple presenters.
        /// </summary>
        public List<PresenterConfiguration> presenters;

        /// <summary>
        /// The material factory allows the UG System to take function with any render pipeline, provided that
        /// the appropriate material factory has been created and has been assigned.
        /// 
        /// The Unity Geospatial Framework comes with material factories for both the Built-in render pipeline
        /// as well as the Universal Render Pipeline.
        /// </summary>
        public UGMaterialFactoryObject materialFactory;

        /// <summary>
        /// The streaming mode is an advanced feature that allows the <see cref="UGSystemBehaviour"/> to trade off
        /// impact on the simulation's framerate in exchange for faster streaming.
        /// </summary>
        public UGSystem.StreamingModes streamingMode;

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
        public float mainThreadTimeLimitMS = 10;

        /// <summary>
        /// This is the amount of files that will be loaded simultaneously.
        /// If the limit has been reached, items part of the queue needs to be completed before new ones can be added.
        /// </summary>
        /// <remarks>
        /// The higher the value, faster execution time you will get. But if the value is too high, slower
        /// it will get since the system will not be able to adapt when <see cref="UGSceneObserver"/> are in movement.
        /// If the value is too low, it will take more time to get a higher resolution, but you will gain reaction speed.
        /// </remarks>
        public int maximumSimultaneousContentRequests = 10;

        /// <summary>
        /// The approximate radius of the planet. By default, this setting is configured such that it matches 
        /// the radius of the earth and can be fetched by various scripts for things like adjusting clipping
        /// planes based off of the camera's altitude or configuring the skybox's appearance.
        /// </summary>
        public float planetRadius = 6360023.0f;

        /// <summary>
        /// <see cref="UGSystem"/> this instance interact with.
        /// </summary>
        private UGSystem m_UGSystem;

        /// <summary>
        /// <see cref="UGDataSource"/> requested to be loaded.
        /// </summary>
        private UGDataSource[] m_InitializedDataSources;

        /// <summary>
        /// <see cref="UGDataSource"/> instances available for streaming content.
        /// </summary>
        public UGDataSource[] InitializedDataSources
        {
            get
            {
                return m_InitializedDataSources ??= dataSources
                    .Select(ds => ds.Instantiate(this))
                    .ToArray();
            }
        }

        /// <summary>
        /// Reset the instance to its default values and stop using the currently set values.
        /// </summary>
        private void Reset()
        {
            presenters = new List<PresenterConfiguration>();
            PresenterConfiguration defaultPresenter;
            defaultPresenter.dataSources = -1;
            defaultPresenter.outputLayer = 0;
            defaultPresenter.outputRoot = transform;
            presenters.Add(defaultPresenter);

        }

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// <seealso href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html">MonoBehaviour.OnEnable</seealso>
        /// </summary>
        private void OnEnable()
        {
            UGSystem.Configuration configuration;

            if (materialFactory == null)
            {
                Debug.LogError("Material Factory not set on UGSystem component");
                return;
            }

            configuration.MaterialFactory = materialFactory.Instantiate();

            UGBehaviourPresenter[] instantiatedPresenters = presenters
                            .Select(conf => new UGBehaviourPresenter(conf.outputRoot, conf.outputLayer, GetDataSourceIDs(conf.dataSources)))
                            .ToArray();
            
            configuration.Presenters = instantiatedPresenters;

            for (int i = 0; i < instantiatedPresenters.Length; i++)
            {
                instantiatedPresenters[i].OnAllocateInstance += (id, gameObject) => OnAllocateInstance?.Invoke(i, id, gameObject);
                instantiatedPresenters[i].OnDisposeInstance += (id, gameObject) => OnDisposeInstance?.Invoke(i, id, gameObject);
                instantiatedPresenters[i].OnUpdateInstanceVisibility += (id, gameObject, isVisible) => OnUpdateInstanceVisibility?.Invoke(i, id, gameObject, isVisible);
            }

            configuration.StreamingMode = streamingMode;
            configuration.MainThreadTimeLimitMs = mainThreadTimeLimitMS;
            configuration.MaximumSimultaneousContentRequests = maximumSimultaneousContentRequests;

            configuration.DataSources = InitializedDataSources;

            configuration.SceneObservers = sceneObservers.Select(o => o.Instantiate(this)).ToArray();
            configuration.Modifiers = modifiers.Select(m => m.Instantiate()).ToArray();

            try
            {
                m_UGSystem = new UGSystem(ref configuration);
            }
            catch (InvalidOperationException e)
            {
                Debug.LogException(e);
                return;
            }

            m_UGSystem.OnBeginProcessing += OnProcessing;
            m_UGSystem.OnEndProcessing += OnProcessed;
        }

        /// <summary>
        /// Called when the object becomes disabled.
        /// This is also called when the object is destroyed and can be used for any cleanup code.
        /// When scripts are reloaded after compilation has finished, OnDisable will be called, followed by an OnEnable
        /// after the script has been loaded.
        /// <seealso href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html">MonoBehaviour.OnDisable</seealso>
        /// </summary>
        private void OnDisable()
        {
            m_UGSystem?.Dispose();
        }

        /// <summary>
        /// Called when a <see cref="UGProcessingNode"/> starts to be processed and none where previously.
        /// </summary>
        /// <param name="eventArgs">Arguments attached to the event.</param>
        private void OnProcessing(SystemProcessingEventArgs eventArgs)
        {
            OnBeginProcessing?.Invoke(eventArgs);
        }

        /// <summary>
        /// Called when all <see cref="UGProcessingNode"/> got executed and no more are pending to be executed.
        /// </summary>
        /// <param name="eventArgs">Arguments attached to the event.</param>
        private void OnProcessed(SystemProcessingEventArgs eventArgs)
        {
            OnEndProcessing?.Invoke(eventArgs);
        }

        /// <summary>
        /// Get an <see langword="Array"/> of <see cref="UGDataSourceID"/> based on the <see cref="InitializedDataSources"/>.
        /// </summary>
        /// <param name="dataSourceMask">The selected <see cref="UGDataSource"/> by the <see cref="UGPresenter"/>.</param>
        /// <returns>
        /// The <see langword="Array"/> of <see cref="UGDataSourceID"/> representing the expected <see cref="UGDataSource"/> to be loaded.
        /// </returns>
        public List<UGDataSourceID> GetDataSourceIDs(int dataSourceMask)
        {
            UGDataSource[] sources = InitializedDataSources;
            List<UGDataSourceID> result = new List<UGDataSourceID>();

            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] != null && ((0x00000001 << i) & dataSourceMask) != 0)
                    result.Add(sources[i].DataSourceID);
            }

            return result;
        }

        /// <summary>
        /// Called every frame if enabled.
        /// <seealso href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">MonoBehaviour.Update</seealso>
        /// </summary>
        private void Update()
        {
            m_UGSystem?.ProcessFrame();
        }

    }
}
