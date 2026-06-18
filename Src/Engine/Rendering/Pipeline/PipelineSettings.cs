namespace SpatialSim.Engine.Rendering
{
    public enum RasterizationMode
    {
        Triangle,
        Line
    }

    public struct PipelineSettings
    {
        public RasterizationMode rasterizationMode;
        /// <summary>
        /// Blends color with past pipeline drawn
        /// </summary>
        public bool blendColor;
        /// <summary>
        /// Enables depth writing and shit
        /// </summary>
        public bool depthTest;
        /// <summary>
        /// Format that will be rendered too, Pipelines that render to the render texture must follow the format of the render texture
        /// </summary>
        public TextureFormat format;
    }
}