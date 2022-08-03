using System;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// The content type is used to figure out which NodeContent classes are
    /// associated to which ContentLoaders.
    /// </summary>
    public readonly struct ContentType : IEquatable<ContentType>
    {
        //
        //  0-9 are reserved for control types
        //

        /// <summary>
        /// A content type reserved for empty nodes in the BVH.
        /// </summary>
        public static readonly ContentType EmptyNode = new ContentType(0);

        /// <summary>
        /// The starting point of the automatically generated ContentType Ids.
        /// </summary>
        internal const int UnreservedStart = 10;

        /// <summary>
        /// A unique id generated for each content type.
        /// </summary>
        private int Id { get; }

        /// <summary>
        /// Default constructor. This should not be used directly and should really
        /// only be used by the UniqueContentTypeGenerator.
        /// </summary>
        /// <param name="id">The unique id to be used for the given content type.</param>
        public ContentType(int id)
        {
            Id = id;
        }

        /// <summary>
        /// See if this is equivalent to another object.
        /// </summary>
        /// <param name="obj">The other object</param>
        /// <returns>True if the other object is a ContentType class and has the same id.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ContentType))
                return false;

            return Equals((ContentType)obj);
        }

        /// <summary>
        /// See if two content type classes are equivalent.
        /// </summary>
        /// <param name="obj">The other content type.</param>
        /// <returns>True if they both have the same id.</returns>
        public bool Equals(ContentType obj)
        {
            return obj.Id == Id;
        }

        /// <summary>
        /// Obtain the hash of the ContentType. Essentially returns
        /// the id.
        /// </summary>
        /// <returns>The hash of the ContentType.</returns>
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
