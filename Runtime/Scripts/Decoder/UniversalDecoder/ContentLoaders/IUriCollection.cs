
using System;
using System.Collections.Generic;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Collection of paths defining the source of data to load when <see cref="UriLoader.LoadAsync"/> is called
    /// for a single <see cref="ILeaf"/>.
    /// </summary>
    public interface IUriCollection:
        IEnumerable<Uri>
    {
        /// <summary>
        /// Get the <see cref="FileType"/> associated with this collection. Based on the returned value,
        /// the corresponding <see cref="UriLoader"/> will be used when
        /// <see cref="UriLoader.LoadAsync"/> / <see cref="UriLoader.Unload"/> is called.
        /// </summary>
        /// <remarks>This is usually the <see cref="FileType"/> associated with the <see cref="MainUri"/>.</remarks>
        /// <returns>The <see cref="FileType"/> used to get the corresponding registered <see cref="UriLoader"/>.</returns>
        FileType GetFileType();

        /// <summary>
        /// Get if the instance can be loaded.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if valid links are part of this instance;
        /// <see langword="false"/> if required links are not set.
        /// </returns>
        bool HasContent();

        /// <summary>
        /// The most representative link part of this collection.
        /// </summary>
        Uri MainUri { get; }
    }
}
