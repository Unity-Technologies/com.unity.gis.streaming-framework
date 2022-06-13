using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Unity.Geospatial.Streaming.UniversalDecoder;

namespace Unity.Geospatial.Streaming.UnityTerrain
{
    /// <summary>
    /// <see cref="IUriCollection"/> to use when loading Unity Terrain datasets.
    /// </summary>
    public readonly struct UriCollection :
        IUriCollection,
        IEquatable<UriCollection>
    {
        /// <summary>
        /// Link to access the geometry data.
        /// </summary>
        public Uri MeshUri { get; }

        /// <summary>
        /// Link to access the data of the albedo texture.
        /// </summary>
        public Uri AlbedoUri { get; }

        /// <summary>
        /// Constructor with the URIs already fully constructed.
        /// </summary>
        /// <param name="meshUri">Link where to access the mesh data.</param>
        /// <param name="albedoUri">Link where to access the albedo texture.</param>
        /// <param name="baseUri">An absolute Uri that is the base for the new mesh Uri instance.
        /// If the <paramref name="albedoBaseUri"/> is not set, the baseUri will also be the albedo base uri.</param>
        /// <param name="albedoBaseUri">An absolute Uri that is the base for the new albedo Uri instance.</param>
        public UriCollection(string meshUri, string albedoUri, Uri baseUri = null, Uri albedoBaseUri = null)
        {
            MeshUri = string.IsNullOrEmpty(meshUri)
                ? null
                : PathUtility.StringToUri(meshUri, baseUri);

            if (string.IsNullOrEmpty(albedoUri))
                AlbedoUri = null;
            else if (albedoBaseUri is null)
                AlbedoUri = PathUtility.StringToUri(albedoUri, baseUri);
            else
                AlbedoUri = PathUtility.StringToUri(albedoUri, albedoBaseUri);
        }

        /// <summary>
        /// Constructor with the URIs already fully constructed and base Uris as strings.
        /// </summary>
        /// <param name="meshUri">Link where to access the mesh data.</param>
        /// <param name="albedoUri">Link where to access the albedo texture.</param>
        /// <param name="baseUri">An absolute Uri that is the base for the new mesh Uri instance.
        /// If the <paramref name="albedoBaseUri"/> is not set, the baseUri will also be the albedo base uri.</param>
        /// <param name="albedoBaseUri">An absolute Uri that is the base for the new albedo Uri instance.</param>
        public UriCollection(string meshUri, string albedoUri, string baseUri, string albedoBaseUri = null) :
            this(meshUri, albedoUri, baseUri is null ? null : new Uri(baseUri), albedoBaseUri is null ? null : new Uri(albedoBaseUri))
        { }

        /// <summary>
        /// Constructor with all the elements needed to construct the URIs based on the UnityTerrain template.
        /// </summary>
        /// <param name="level">Correspond to <see cref="Tile.Level"/>.</param>
        /// <param name="column">Correspond to <see cref="Tile.Column"/>.</param>
        /// <param name="row">Correspond to <see cref="Tile.Row"/>.</param>
        /// <param name="meshPrefix">Correspond to <see cref="ContentSchema.TerrainUri"/>.</param>
        /// <param name="meshExtension">Correspond to <see cref="ContentSchema.TerrainFormat"/>.</param>
        /// <param name="albedoPrefix">Correspond to <see cref="ContentSchema.ImageryUri"/>.</param>
        /// <param name="albedoExtension">Correspond to <see cref="ContentSchema.ImageryFormat"/>.</param>
        /// <param name="baseUri">An absolute Uri that is the base for the new mesh Uri instance.
        /// If the <paramref name="albedoBaseUri"/> is not set, the baseUri will also be the albedo base uri.</param>
        /// <param name="albedoBaseUri">An absolute Uri that is the base for the new albedo Uri instance.</param>
        public UriCollection(int level, uint column, uint row, string meshPrefix, string meshExtension, string albedoPrefix, string albedoExtension, Uri baseUri = null, Uri albedoBaseUri = null) :
            this(
                $"{Path.Combine(meshPrefix, level.ToString(), column.ToString(), row.ToString())}.{meshExtension}",
                $"{Path.Combine(albedoPrefix, level.ToString(), column.ToString(), row.ToString())}.{albedoExtension}",
                baseUri,
                albedoBaseUri)
        { }

        /// <summary>
        /// Constructor with a <see cref="ContentSchema"/> instance.
        /// </summary>
        /// <param name="level">Correspond to <see cref="Tile.Level"/>.</param>
        /// <param name="column">Correspond to <see cref="Tile.Column"/>.</param>
        /// <param name="row">Correspond to <see cref="Tile.Row"/>.</param>
        /// <param name="content">Complete the URIs based on the values of this instance.</param>
        /// <param name="baseUri">An absolute Uri that is the base for the new mesh Uri instance.
        /// If the <paramref name="albedoBaseUri"/> is not set, the baseUri will also be the albedo base uri.</param>
        /// <param name="albedoBaseUri">An absolute Uri that is the base for the new albedo Uri instance.</param>
        public UriCollection(int level, uint column, uint row, ContentSchema content, Uri baseUri = null, Uri albedoBaseUri = null) :
            this(level, column, row, content.TerrainUri, content.TerrainFormat, content.ImageryUri, content.ImageryFormat, baseUri, albedoBaseUri)
        { }

        /// <summary>
        /// Get if two objects have the same values.
        /// </summary>
        /// <param name="obj">Other collection to compare with.</param>
        /// <returns>
        /// <see langword="true"/> if both objects point to the same uris;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is UriCollection uri)
                return Equals(uri);
            
            return false;
        }

        /// <summary>
        /// Get if two <see cref="UriCollection"/> have the same values.
        /// </summary>
        /// <param name="obj">Other collection to compare with.</param>
        /// <returns>
        /// <see langword="true"/> if both objects point to the same uris;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(UriCollection other)
        {
            return MeshUri == other.MeshUri && AlbedoUri == other.AlbedoUri;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Uri collection.
        /// </summary>
        public IEnumerator<Uri> GetEnumerator()
        {
            yield return MeshUri;
            yield return AlbedoUri;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Uri collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Get the <see cref="FileType"/> associated with this collection. Based on the returned value,
        /// the corresponding <see cref="UriLoader"/> will be used when
        /// <see cref="UriLoader.LoadAsync"/> / <see cref="UriLoader.Unload"/> is called.
        /// </summary>
        /// <returns>The <see cref="FileType"/> associated with the <see cref="MeshUri"/>.</returns>
        public FileType GetFileType()
        {
            return FileType.GetByFileExtension(MeshUri?.AbsolutePath);
        }

        /// <summary>
        /// Compute a hash code for the object.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return
                    ((MeshUri is null ? 0 : MeshUri.GetHashCode()) * 397)
                    ^ (AlbedoUri is null ? 0 : AlbedoUri.GetHashCode());
            }
        }

        /// <summary>
        /// Get if the instance can be loaded.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the collection has a <see cref="MeshUri"/>;
        /// <see langword="false"/> if <see cref="MeshUri"/> is <see langword="null"/> or an empty <see langword="string"/>.
        /// </returns>
        public bool HasContent()
        {
            return !(MeshUri is null);
        }

        /// <summary>
        /// Get the value of <see cref="MeshUri"/>.
        /// </summary>
        Uri IUriCollection.MainUri
        {
            get { return MeshUri; }
        }
    }
}
