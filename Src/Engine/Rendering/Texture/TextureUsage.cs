namespace SpatialSim.Engine.Rendering
{
    [Flags]
    public enum TextureUsage
    {
        //Hot encoded
        Sampler = 0,
        Storage = 1 << 0,
        ColorFramebuffer = 1 << 1,
    }
}