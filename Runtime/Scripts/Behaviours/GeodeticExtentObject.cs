using System;
using System.Collections.Generic;

using UnityEngine;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// ScriptableObject of a <see cref="GeodeticExtent"/> allowing the user to define a region polygon using geodetic coordinates (latitude / longitude).
    /// to be loaded.
    /// </summary>
    [CreateAssetMenu(fileName = "GeodeticExtent", menuName = "Geospatial/Data Types/Geodetic Extent", order = UGDataSourceObject.AssetMenuOrder)]
    public class GeodeticExtentObject : ScriptableObject
    {
        /// <summary>
        /// Returns the list of points that the extent is comprised of.
        /// These are guaranteed to be clockwise.
        /// </summary>
        public List<double2> Points;

        /// <summary>
        /// Create a new <see cref="GeodeticExtent"/> instance representing this ScriptableObject.
        /// </summary>
        /// <returns>The newly created instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the <see cref="Points"/> did not pass the <see cref="GeodeticExtent.ValidateExtent">Validation</see>.
        /// </exception>
        public GeodeticExtent Instantiate()
        {
            GeodeticExtent result = new GeodeticExtent(Points);

            if (!result.IsValid)
                throw new InvalidOperationException("Geodetic extent is invalid");

            return result;
        }
    }
}
