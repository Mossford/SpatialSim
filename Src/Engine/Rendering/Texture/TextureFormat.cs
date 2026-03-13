namespace SpatialSim.Engine.Rendering
{
    public enum TextureFormat
    {
        R8G8B8A8Unorm, //Swapchain format
        R8G8B8A8Srgb,
        //My laptop does not support the 3 value format, so use the 4 value by default
        R8G8B8Uint,
        R8G8B8A8Uint
    }
}