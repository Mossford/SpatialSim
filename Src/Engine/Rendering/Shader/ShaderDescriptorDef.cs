namespace SpatialSim.Engine.Rendering
{
    public enum ShaderDescriptorUsage
    {
        Uniform,
        Sampler
    }
    
    public struct ShaderDescriptorDef
    {
        public int set;
        public int binding;
        public ShaderDescriptorUsage usage;

        public ShaderDescriptorDef(int set, int binding, ShaderDescriptorUsage usage)
        {
            this.set = set;
            this.binding = binding;
            this.usage = usage;
        }
    }
}