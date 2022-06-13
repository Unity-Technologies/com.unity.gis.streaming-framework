
namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Default Batched 3D Model Feature Table schema.
    /// </summary>
    public class B3dmTableSchema
    {
        /// <summary>
        /// The number of distinguishable models, also called features, in the batch.
        /// If the Binary glTF does not have a batchId attribute, this field must be 0.
        /// </summary>
        public int Batch_Length { get; set; }

        /// <summary>
        /// A 3-component array of numbers defining the center position when positions are defined relative-to-center.
        /// </summary>
        public double[] Rtc_Center { get; set; }
    }
}
