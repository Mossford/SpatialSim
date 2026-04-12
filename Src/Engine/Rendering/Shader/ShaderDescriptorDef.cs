namespace SpatialSim.Engine.Rendering
{
    public enum ShaderDescriptorUsage
    {
        Uniform,
        Sampler
    }
    
    public struct ShaderDescriptorDef : IEquatable<ShaderDescriptorDef>
    {
        public int set;
        public int[] bindings;
        public ShaderDescriptorUsage usage;
        public ShaderType type;
        
        public ShaderDescriptorDef(int set, int[] bindings, ShaderDescriptorUsage usage, ShaderType type)
        {
            this.set = set;
            this.bindings = bindings;
            this.usage = usage;
            this.type = type;
        }

        public override string ToString()
        {
            return $"<{set}, <{string.Join(',', bindings)}>, {usage}, {type}>";
        }

        public bool Equals(ShaderDescriptorDef other)
        {
            return set == other.set && usage == other.usage && type == other.type;
        }

        public override bool Equals(object? obj)
        {
            return obj is ShaderDescriptorDef other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(set, (int)usage, (int)type);
        }
    }
}