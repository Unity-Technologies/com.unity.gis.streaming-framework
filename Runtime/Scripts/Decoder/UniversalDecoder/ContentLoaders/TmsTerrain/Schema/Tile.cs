
using System;
using System.Collections.Generic;

using Unity.Geospatial.HighPrecision;
using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming.TmsTerrain
{
    /// <summary>
    /// Represent a single tile part of a TMS (Terrain Management System) Terrain dataset. This contains all the information needed
    /// to load terrain and imagery data representing a single tile for a specific zoom level.
    /// </summary>
    public class Tile :
        ILeaf
    {

        /// <summary>
        /// Default elevation to be used to calculate the bounding volume of the root node
        /// of the tileset. This elevation is in meters relative to the WGS84 ellipsoid.
        /// </summary>
        private const double k_DefaultRootMinElevation = -5000.0;

        /// <summary>
        /// Default elevation to be used to calculate the bounding volume of the root node
        /// of the tileset. This elevation is in meters relative to the WGS84 ellipsoid.
        /// </summary>
        private const double k_DefaultRootMaxElevation = 5000.0;

        /// <summary>
        /// Define how to format the <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> part of this dataset.
        /// </summary>
        private ContentSchema Content { get; }

        /// <summary>
        /// The latitude column index this tile represent.
        /// The index must be between <see cref="Limits"/>.<see cref="LimitsSchema.MinCol">MinCol</see> and
        /// <see cref="Limits"/>.<see cref="LimitsSchema.MinCol">MaxCol</see>.
        /// </summary>
        private uint Column { get; }

        /// <summary>
        /// Define the coordinate limits of a tile based on the WGS84 system.
        /// </summary>
        private ExtentSchema Extent { get; set; }

        /// <summary>
        /// The error, in meters, introduced if this item is rendered and its children are not.
        /// At runtime, the geometric error is used to compute screen space error (SSE), i.e., the error measured in pixels.
        /// </summary>
        private float GeometricError { get; set; }

        /// <summary>
        /// <see langword="true"/> if the tile has children tiles;
        /// <see langword="false"/> this tile has the highest available <see cref="Level"/>.
        /// </summary>
        private bool HasChildren { get; set; }

        /// <summary>
        /// The zoom level (precision) of this tile.
        /// Closer to zero the value is, less detailed it is.
        /// </summary>
        private int Level { get; }

        /// <summary>
        /// Layout information on how to divide the tile.
        /// </summary>
        private LimitsSchema Limits { get; }

        /// <summary>
        /// Lower possible elevation.
        /// </summary>
        private double MinElevation { get; set; }

        /// <summary>
        /// Highest possible elevation.
        /// </summary>
        private double MaxElevation { get; set; }

        /// <summary>
        /// The longitude row index this tile represent.
        /// The index must be between <see cref="Limits"/>.<see cref="LimitsSchema.MinRow">MinRow</see> and
        /// <see cref="Limits"/>.<see cref="LimitsSchema.MinRow">MaxRow</see>.
        /// </summary>
        private uint Row { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="level">The zoom level (precision) of this tile. Closer to zero the value is, less detailed it is.</param>
        /// <param name="column">The latitude column index this tile represent.
        /// The index must be between <see cref="Limits"/>.<see cref="LimitsSchema.MinCol">MinCol</see> and
        /// <see cref="Limits"/>.<see cref="LimitsSchema.MinCol">MaxCol</see>.</param>
        /// <param name="row">The longitude row index this tile represent.
        /// The index must be between <see cref="Limits"/>.<see cref="LimitsSchema.MinRow">MinRow</see> and
        /// <see cref="Limits"/>.<see cref="LimitsSchema.MinRow">MaxRow</see>.</param>
        /// <param name="geometricError">The error, in meters, introduced if this item is rendered and its children are not.
        /// At runtime, the geometric error is used to compute screen space error (SSE), i.e., the error measured in pixels.</param>
        /// <param name="content">
        /// Define how to format the <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> part
        /// of this dataset.</param>
        /// <param name="extent">Define the coordinate limits of a tile based on the WGS84 system.</param>
        /// <param name="limits">Layout information on how to divide the tile.</param>
        /// <param name="minElevation">Lower possible elevation.</param>
        /// <param name="maxElevation">Highest possible elevation.</param>
        public Tile(
            int level,
            uint column,
            uint row,
            float geometricError,
            ContentSchema content,
            ExtentSchema extent,
            LimitsSchema limits,
            double minElevation = k_DefaultRootMinElevation,
            double maxElevation = k_DefaultRootMaxElevation)
        {
            Assert.IsNotNull(content, "Content is null.");
            Assert.IsNotNull(extent, "Extent is null.");
            Assert.IsNotNull(limits, "Limits is null.");

            Level = level;
            Column = column;
            Row = row;
            GeometricError = geometricError;
            Content = content;
            Extent = extent;
            Limits = limits;
            MinElevation = minElevation;
            MaxElevation = maxElevation;
            HasChildren = level < limits.MaxLevel;
        }

        /// <summary>
        /// Constructor with an already decoded utr file.
        /// </summary>
        /// <param name="level">The zoom level (precision) of this tile. Closer to zero the value is, less detailed it is.</param>
        /// <param name="column">The latitude column index this tile represent.
        /// The index must be between <see cref="Limits"/>.<see cref="LimitsSchema.MinCol">MinCol</see> and
        /// <see cref="Limits"/>.<see cref="LimitsSchema.MinCol">MaxCol</see>.</param>
        /// <param name="row">The longitude row index this tile represent.
        /// The index must be between <see cref="Limits"/>.<see cref="LimitsSchema.MinRow">MinRow</see> and
        /// <see cref="Limits"/>.<see cref="LimitsSchema.MinRow">MaxRow</see>.</param>
        /// <param name="content">
        /// Define how to format the <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> part
        /// of this dataset.</param>
        /// <param name="limits">Layout information on how to divide the tile.</param>
        /// <param name="terrain">Decoded TMS (Terrain Management System) Terrain file.</param>
        public Tile(int level, uint column, uint row, ContentSchema content, LimitsSchema limits, TmsTerrainStreamer.TerrainTile terrain)
        {
            Assert.IsNotNull(content, "Content is null.");
            Assert.IsNotNull(limits, "Limits is null.");
            Assert.IsNotNull(terrain, "TerrainTile is null.");

            Level = level;
            Column = column;
            Row = row;
            GeometricError = terrain.GeometricError;
            Content = content;
            Extent = terrain.Extent;
            Limits = limits;
            MinElevation = terrain.MinElevation;
            MaxElevation = terrain.MaxElevation;
            HasChildren = level < limits.MaxLevel && terrain.HasChildren();
        }

        /// <inheritdoc cref="ILeaf.GetBoundingVolume(double4x4)"/>
        public DoubleBounds GetBoundingVolume(double4x4 transform)
        {
            return Extent.ToBoundingVolume(transform, MinElevation, MaxElevation);
        }

        /// <inheritdoc cref="ILeaf.GetChildren()"/>
        public IEnumerable<ILeaf> GetChildren()
        {
            return Level < 0
                ? GetRootChildren()
                : GetQuadChildren();
        }

        /// <inheritdoc cref="ILeaf.GetGeometricError()"/>
        public float GetGeometricError()
        {
            return GeometricError;
        }

        /// <summary>
        /// Enumerate child items of this parent by dividing the tile in four equal parts (North-West, North-East, South-West, South-East).
        /// </summary>
        /// <returns>Four child tiles if the zoom <see cref="Level"/> is not the last.</returns>
        private IEnumerable<Tile> GetQuadChildren()
        {
            if (!HasChildren)
                yield break;

            int level = Level + 1;
            uint colW = Column * 2;
            uint colE = Column * 2 + 1;
            uint rowN = Row * 2 + 1;
            uint rowS = Row * 2;

            double midLat = Extent.MinLat + 0.5 * (Extent.MaxLat - Extent.MinLat);
            double midLon = Extent.MinLon + 0.5 * (Extent.MaxLon - Extent.MinLon);

            yield return new Tile(
                level,
                colW,
                rowS,
                0,
                Content,
                new ExtentSchema(Extent.MinLat, midLat, Extent.MinLon, midLon),
                Limits,
                MinElevation,
                MaxElevation);

            yield return new Tile(
                level,
                colW,
                rowN,
                0,
                Content,
                new ExtentSchema(midLat, Extent.MaxLat, Extent.MinLon, midLon),
                Limits,
                MinElevation,
                MaxElevation);

            yield return new Tile(
                level,
                colE,
                rowS,
                0,
                Content,
                new ExtentSchema(Extent.MinLat, midLat, midLon, Extent.MaxLon),
                Limits,
                MinElevation,
                MaxElevation);

            yield return new Tile(
                level,
                colE,
                rowN,
                0,
                Content,
                new ExtentSchema(midLat, Extent.MaxLat, midLon, Extent.MaxLon),
                Limits,
                MinElevation,
                MaxElevation);
        }

        /// <inheritdoc cref="ILeaf.GetRefinement(RefinementMode)"/>
        public RefinementMode GetRefinement(RefinementMode inherited)
        {
            return RefinementMode.Replace;
        }

        /// <summary>
        /// Enumerate child items of this parent based on the <see cref="Limits"/>.
        /// </summary>
        /// <returns>The child <see cref="Tile">Tiles</see>.</returns>
        private IEnumerable<Tile> GetRootChildren()
        {
            int level = Limits.MinLevel;
            double rowSize = (Extent.MaxLat - Extent.MinLat) / math.pow(2, level);
            double colSize = (Extent.MaxLon - Extent.MinLon) / math.pow(2, level + 1);

            for (uint col = Limits.MinCol; col <= Limits.MaxCol; col++)
            {
                for (uint row = Limits.MinRow; row <= Limits.MaxRow; row++)
                {
                    double minLat = Extent.MinLat + rowSize * row;
                    double maxLat = minLat + rowSize;
                    double minLon = Extent.MinLon + colSize * col;
                    double maxLon = minLon + colSize;

                    yield return new Tile(
                        level,
                        col,
                        row,
                        0,
                        Content,
                        new ExtentSchema(minLat, maxLat, minLon, maxLon),
                        Limits,
                        MinElevation,
                        MaxElevation);
                }
            }
        }

        /// <inheritdoc cref="ILeaf.GetTransform()"/>
        public double4x4 GetTransform()
        {
            double latCenter = 0.5 * (Extent.MinLat + Extent.MaxLat);
            double lonCenter = 0.5 * (Extent.MinLon + Extent.MaxLon);
            double elevationCenter = 0.5 * (MinElevation + MaxElevation);

            GeodeticCoordinates tileCenter = new GeodeticCoordinates(latCenter, lonCenter, elevationCenter);

            return Wgs84.GetXzyEcefFromXzyEnuMatrix(tileCenter);
        }

        /// <inheritdoc cref="ILeaf.GetUriCollection()"/>
        public IUriCollection GetUriCollection(Uri baseUri)
        {
            return Level < 0
                ? new UriCollection(null, null)
                : new UriCollection(Level, Column, Row, Content, baseUri);
        }

        /// <summary>
        /// Update the properties based on the given <paramref name="terrain"/> values.
        /// </summary>
        /// <param name="terrain">Update the <see cref="Tile"/> values based on the values of this instance.</param>
        public void Update(TmsTerrainStreamer.TerrainTile terrain)
        {
            GeometricError = terrain.GeometricError;
            Extent = terrain.Extent;
            MinElevation = terrain.MinElevation;
            MaxElevation = terrain.MaxElevation;
            HasChildren = Level < Limits.MaxLevel && terrain.HasChildren();
        }
    }
}
