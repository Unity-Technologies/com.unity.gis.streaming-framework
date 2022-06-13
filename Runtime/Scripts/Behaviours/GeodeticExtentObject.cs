using System;
using System.Collections.Generic;

using UnityEngine;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    [CreateAssetMenu(fileName = "GeodeticExtent", menuName = "Geospatial/Data Types/Geodetic Extent", order = UGDataSourceObject.AssetMenuOrder)]
    public class GeodeticExtentObject : ScriptableObject
    {
        public List<double2> Points;

        public GeodeticExtent Instantiate()
        {
            GeodeticExtent result = new GeodeticExtent(Points);

            if (!result.IsValid)
                throw new InvalidOperationException("Geodetic extent is invalid");

            return result;
        }
    }
}
