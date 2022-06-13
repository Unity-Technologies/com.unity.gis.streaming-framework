using System.Collections.Generic;

using UnityEngine;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    public interface IUGRenderer : System.IDisposable
    {
        /// <summary>
        /// Sets whether the renderer is enabled or not. This should translate
        /// into the GameObject or Entity being enabled or not.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Sets the name of the renderer for debugging purposes.
        /// </summary>
        string Name { set; }

        /// <summary>
        /// Sets the Unity Layer as defined in the project settings. This integer should
        /// be from 0 to 31.
        /// </summary>
        int UnityLayer { set; }

        /// <summary>
        /// Sets the parent of the GameObject within the scene hierarchy.
        /// </summary>
        Transform Parent { set; }

        /// <summary>
        /// Enables high precision on this renderer. If set to false, position will
        /// be maintained with a single precision floating point. If set to true,
        /// position will be maintained with a double precision floating point.
        /// </summary>
        bool EnableHighPrecision { set; }

        /// <summary>
        /// Get the list of child renderers
        /// </summary>
        List<IUGRenderer> Children { get; }

        /// <summary>
        /// Add a child renderer
        /// </summary>
        /// <param name="child"></param>
        void AddChild(IUGRenderer child);

        /// <summary>
        /// Remove a child renderer
        /// </summary>
        /// <param name="child"></param>
        void RemoveChild(IUGRenderer child);

        /// <summary>
        /// Clear all child renderers. This will not delete the children
        /// but rather unparent them to facilitate pooling systems.
        /// </summary>
        void ClearChildren();

        /// <summary>
        /// Set the transform of the renderer. If EnableHighPrecision is not set,
        /// this will be reduced to single precision floating point.
        /// </summary>
        double4x4 Transform { set; }

        /// <summary>
        /// Set the renderer's metadata
        /// </summary>
        UGMetadata Metadata { set; }

        /// <summary>
        /// Set the renderer's mesh
        /// </summary>
        Mesh Mesh { set; }

        /// <summary>
        /// Set the renderer's material list
        /// </summary>
        UGMaterial[] Materials { set; }
    }
}
