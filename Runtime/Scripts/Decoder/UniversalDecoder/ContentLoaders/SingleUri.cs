
using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// <see cref="IUriCollection"/> with only one URI part of it.
    /// </summary>
    public readonly struct SingleUri :
        IUriCollection,
        IEquatable<SingleUri>
    {
        /// <summary>
        /// The path to use when <see cref="UriLoader.LoadAsync"/> is called with this collection.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Constructor with a Uri as a string.
        /// </summary>
        /// <param name="relativeUri">A relative Uri instance that is combined with baseUri.</param>
        /// <param name="baseUri">An absolute Uri that is the base for the new Uri instance.</param>
        public SingleUri(string relativeUri, Uri baseUri = null)
        {
            Uri = string.IsNullOrEmpty(relativeUri)
                ? null
                : PathUtility.StringToUri(relativeUri, baseUri);
        }

        /// <summary>
        /// Constructor with a base and a relative uris allowing to construct a new uri.
        /// </summary>
        /// <param name="relativeUri">A relative Uri instance that is combined with baseUri.</param>
        /// <param name="baseUri">An absolute Uri that is the base for the new Uri instance.</param>
        public SingleUri(Uri relativeUri, Uri baseUri = null)
        {
            if (relativeUri is null)
                Uri = null;
            else if (baseUri is null)
                Uri = relativeUri;
            else
                Uri = new Uri(baseUri, relativeUri);
        }

        /// <summary>
        /// Constructor with a Uri as a string.
        /// </summary>
        /// <param name="relativeUri">A relative Uri instance that is combined with baseUri.</param>
        /// <param name="baseUri">An absolute Uri that is the base for the new Uri instance.</param>
        public SingleUri(string relativeUri, string baseUri) :
            this(relativeUri, string.IsNullOrEmpty(baseUri) ? null : new Uri(baseUri))
        { }

        /// <summary>
        /// Get if two objects have the same values.
        /// </summary>
        /// <param name="obj">Other Uri to compare with.</param>
        /// <returns>
        /// <see langword="true"/> if both objects point to the same uri;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj switch
            {
                SingleUri singleUri => Equals(singleUri),
                Uri uri => Uri == uri,
                string text => Uri.AbsolutePath == text,
                _ => false
            };
        }

        /// <summary>
        /// Get if two <see cref="SingleUri"/> have the same values.
        /// </summary>
        /// <param name="obj">Other Uri to compare with.</param>
        /// <returns>
        /// <see langword="true"/> if both objects point to the same uri;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(SingleUri other)
        {
            return Uri == other.Uri;
        }

        /// <summary>
        /// Get the <see cref="FileType"/> associated with this collection. Based on the returned value,
        /// the corresponding <see cref="UriLoader"/> will be used when
        /// <see cref="UriLoader.LoadAsync"/> / <see cref="UriLoader.Unload"/> is called.
        /// </summary>
        /// <returns>The <see cref="FileType"/> associated with the <see cref="Uri"/>.</returns>
        public FileType GetFileType()
        {
            return FileType.GetByFileExtension(Uri?.AbsolutePath);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Uri collection.
        /// </summary>
        public IEnumerator<Uri> GetEnumerator()
        {
            yield return Uri;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Uri collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Compute a hash code for the object.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return Uri is null ? 0 : Uri.GetHashCode();
        }

        /// <summary>
        /// Get if the instance can be loaded.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the collection has a <see cref="Uri"/>;
        /// <see langword="false"/> if <see cref="Uri"/> is <see langword="null"/> or an empty <see langword="string"/>.
        /// </returns>
        public bool HasContent()
        {
            return !(Uri is null);
        }

        /// <summary>
        /// Get the value of <see cref="Uri"/>.
        /// </summary>
        Uri IUriCollection.MainUri
        {
            get { return Uri; }
        }
    }
}
