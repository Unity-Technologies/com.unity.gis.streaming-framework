using System.Collections.Generic;

using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The extent modifier allow to remove a zone to be displayed for a specified <see cref="UGDataSourceObject"/>.
    /// To do so, you must select a <see cref="GeodeticExtentObject"/>. This will be the shape that will be used to
    /// cut the <see cref="UGDataSourceObject"/>. After, you select <see cref="UGDataSourceObject">sources</see>
    /// in the <see cref="DifferenceDataSources"/> and <see cref="IntersectionDataSources"/> that will be cut the
    /// selected <see cref="Extent"/>. <see cref="UGDataSourceObject">Sources</see> part of the <see cref="DifferenceDataSources"/>
    /// list will have everything inside the <see cref="Extent"/> excluded. And the <see cref="UGDataSourceObject">sources</see>
    /// in the <see cref="IntersectionDataSources"/> list will only keep their geometry that is inside the
    /// <see cref="Extent"/> shape.
    /// </summary>
    public class UGExtentModifierBehaviour : UGModifierBehaviour
    {
        /// <summary>
        /// Message to display when a data source is not part of the linked <see cref="UGSystem"/>.
        /// </summary>
        internal const string k_MessageMissingSource =
            "Extent Modifier {0} Data Source {1} is uninitialized in this UG system";
        
        /// <inheritdoc cref="Extent"/>
        [SerializeField]
        private GeodeticExtentObject m_Extent;

        /// <inheritdoc cref="DifferenceDataSources"/>
        [SerializeField] 
        private List<UGDataSourceObject> m_DifferenceDataSources;

        /// <inheritdoc cref="IntersectionDataSources"/>
        [SerializeField]
        private List<UGDataSourceObject> m_IntersectionDataSources;

        /// <summary>
        /// The extent of the modifier, in decimal degrees. It must be convex and shouldn't span more than
        /// a few hundred kilometers.
        /// </summary>
        public GeodeticExtentObject Extent
        {
            get { return m_Extent; }
            set { m_Extent = value; }
        }

        /// <summary>
        /// List of data sources which will be cut out by the specified extent. This is usually
        /// comprised of a lower detail environment.
        /// </summary>
        public List<UGDataSourceObject> DifferenceDataSources
        {
            get { return m_DifferenceDataSources; }
            set { m_DifferenceDataSources = value; }
        }

        /// <summary>
        /// List of data sources which will be cropped by the specified extent. This is
        /// usually comprised of a higher detail inset.
        /// </summary>
        public List<UGDataSourceObject> IntersectionDataSources
        {
            get { return m_IntersectionDataSources; }
            set { m_IntersectionDataSources = value; }
        }

        /// <summary>
        /// Create a new <see cref="UGExtentModifier"/> instance representing this <see cref="UGExtentModifierBehaviour"/>.
        /// </summary>
        /// <returns>The newly created instance.</returns>
        public override UGModifier Instantiate()
        {
            List<UGDataSourceID> difference = new List<UGDataSourceID>();
            for (int i = 0; i < m_DifferenceDataSources.Count; i++)
            {
                if (m_DifferenceDataSources[i] != null)
                {
                    difference.Add(m_DifferenceDataSources[i].DataSourceID);
                }
            }

            List<UGDataSourceID> intersection = new List<UGDataSourceID>();
            for (int i = 0; i < m_IntersectionDataSources.Count; i++)
            {
                if (m_IntersectionDataSources[i] != null)
                {
                    intersection.Add(m_IntersectionDataSources[i].DataSourceID);
                }
            }

            return new UGExtentModifier(Extent.Instantiate(), difference, intersection);
        }

        /// <summary>
        /// Compare the given <paramref name="system"/>.<see cref="UGSystemBehaviour.dataSources"/> with this instance
        /// <see cref="DifferenceDataSources"/> and <see cref="IntersectionDataSources"/>. If a <see cref="UGDataSourceObject"/>
        /// is not part of the given <paramref name="system"/>, it will return <see langword="false"/>
        /// </summary>
        /// <param name="system">Check if all sources are part of this system.<see cref="UGSystemBehaviour.dataSources"/></param>
        /// <param name="errorMsg">Error returning the first missing <see cref="UGDataSourceObject"/> if any.</param>
        /// <returns>
        /// <see langword="true"/> if all sources part of the given <paramref name="system"/>;
        /// <see langword="false"/> if a <see cref="UGDataSourceObject"/> is missing.
        /// </returns>
        public override bool Validate(UGSystemBehaviour system, out string errorMsg) 
        {
            errorMsg = string.Empty;

            for (int i = 0; i < m_DifferenceDataSources.Count; i++)
            {
                if (!system.dataSources.Contains(m_DifferenceDataSources[i]))
                {
                    errorMsg = string.Format(k_MessageMissingSource, "Difference", i);
                    return false;
                }
            }

            for (int i = 0; i < m_IntersectionDataSources.Count; i++)
            {
                if (!system.dataSources.Contains(m_IntersectionDataSources[i]))
                {
                    errorMsg = string.Format(k_MessageMissingSource, "Intersection", i);
                    return false;
                }
            }

            return true;
        }
    }
}
