using UnityEngine;
using Unity.Geospatial.HighPrecision;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The UGSkybox sets and controls a spherical skybox shader such that it conforms with the
    /// geoid being displayed.
    /// </summary>
    [RequireComponent(typeof(HPTransform))]
    public class UGSkyboxBehaviour : MonoBehaviour
    {
        /// <summary>
        /// The material it should use as the skybox which is automatically populated. 
        /// upon creation in the editor. The available skybox materials are
        /// located at: `com.unity.geospatial.streaming / Runtime / Assets / Materials / Skybox`
        /// </summary>
        public Material skybox;

        /// <summary>
        /// The radius of the planet used by the skybox. Default is 6.3e6 to match earth's radius.
        /// This value is only passed at initialization and has no effect after start.
        /// </summary>
        public float planetRadius = 6.3e6f;

        private void Start()
        {
            //
            //  Clone the material to avoid writing to assets
            //
            skybox = new Material(skybox);
            skybox.SetFloat("_PlanetRadius", planetRadius);
        }

        private void Reset()
        {
#if UNITY_EDITOR
            skybox = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(GeospatialAssets.SphericalSkybox));
#endif
        }

        private void OnValidate()
        {
            RenderSettings.skybox = skybox;
        }

        private void Update()
        {
            Shader.SetGlobalVector("_EcefCenter", transform.position);
        }
    }
}
