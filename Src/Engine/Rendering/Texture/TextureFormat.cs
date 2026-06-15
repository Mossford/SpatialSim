namespace SpatialSim.Engine.Rendering
{
    public enum TextureFormat
    {
        R8G8B8A8Unorm, //Swapchain format
        R8G8B8A8Srgb,
        /// <summary>
        /// Should not use this
        /// </summary>
        R8G8B8Unorm,
        /// <summary>
        /// Should not use this
        /// </summary>
        R8G8B8Srgb,
        R8Unorm,
        R8G8Unorm,
    }

    public static class TextureFormatExtensions
    {
        public static int GetBytePerPixel(this TextureFormat format)
        {
            return format switch
            {
                TextureFormat.R8G8B8Srgb => 3,
                TextureFormat.R8G8B8A8Srgb => 4,
                TextureFormat.R8G8B8A8Unorm => 4,
                TextureFormat.R8G8Unorm => 2,
                TextureFormat.R8Unorm => 1,
                TextureFormat.R8G8B8Unorm => 3,
                _ => 4
            };
        }
    }
}