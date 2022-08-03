using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Store the required data to be loaded for a specified <see cref="InstanceID"/>.
    /// </summary>
    public class InstanceData
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="dataSource">The instance's source ID. This should match the id of UGDataSource object that this
        /// instance originated from.</param>
        /// <param name="transform">The transform of the child. If this is a child instance, this
        /// will be degraded to a single precision matrix.</param>
        /// <param name="mesh">The ID of the mesh as provided by the UGCommandBuffer.
        /// Once the mesh has been instantiated, this ID will be used to
        /// reunite the mesh with the corresponding instance.</param>
        /// <param name="materials">The materials, once they have been instantiated. Null otherwise.</param>
        public InstanceData(UGDataSourceID dataSource, double4x4 transform, MeshID mesh, MaterialID[] materials)
        {
            Transform = transform;
            MeshID = mesh;
            MaterialIDs = materials;
            Source = dataSource;
        }

        /// <summary>
        /// The name of the instance allowing to differentiate its content from other instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The metadata associated to the same <see cref="InstanceID"/>.
        /// </summary>
        public UGMetadata Metadata { get; set; }

        /// <summary>
        /// The instance's source ID. This should match the id of UGDataSource object that this
        /// instance originated from.
        /// </summary>
        public UGDataSourceID Source { get; }

        /// <summary>
        /// Child instances. Note that these will be instantiated with a single
        /// precision transform relative to their high precision parent.
        /// </summary>
        public List<InstanceData> Children { get; private set; }

        /// <summary>
        /// The transform of the child. If this is a child instance, this
        /// will be degraded to a single precision matrix.
        /// </summary>
        public double4x4 Transform { get; }

        /// <summary>
        /// The ID of the mesh as provided by the UGCommandBuffer.
        /// Once the mesh has been instantiated, this ID will be used to
        /// reunite the mesh with the corresponding instance.
        /// </summary>
        public MeshID MeshID { get; }

        /// <summary>
        /// The ID of the materials as provided by the UGCommandBuffer. Once
        /// the materials have been instantiated, these IDs will be used
        /// to reunite the materials with the corresponding instance.
        /// </summary>
        public MaterialID[] MaterialIDs { get; }

        /// <summary>
        /// This will change to true once the instance has been reunited with
        /// its mesh and its materials.
        /// </summary>
        public bool IsRenderable { get; private set; }

        /// <summary>
        /// The mesh, once it has been instantiated. Null otherwise.
        /// </summary>
        public Mesh Mesh { get; set; }

        /// <summary>
        /// The materials, once they have been instantiated. Null otherwise.
        /// </summary>
        public UGMaterial[] Materials { get; private set; }

        /// <summary>
        /// Add a child to the given instance.
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(InstanceData child)
        {
            Assert.IsNotNull(child);

            Children ??= new List<InstanceData>();

            Children.Add(child);
        }

        /// <summary>
        /// Remove a child from the given instance.
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(InstanceData child)
        {
            Assert.IsNotNull(child);
            Children.Remove(child);
        }

        /// <summary>
        /// Convert the instance to a renderable version of itself by
        /// by providing the instantiated mesh and materials.
        /// </summary>
        /// <param name="mesh">The instantiated mesh</param>
        /// <param name="materials">The instantiated materials</param>
        internal void ConvertToRenderable(Mesh mesh, UGMaterial[] materials)
        {
            Assert.IsNotNull(mesh);
            Assert.IsNotNull(materials);
            Assert.AreEqual(mesh.subMeshCount, materials.Length);
            Mesh = mesh;
            Materials = materials;
            IsRenderable = true;
        }
    }
}
