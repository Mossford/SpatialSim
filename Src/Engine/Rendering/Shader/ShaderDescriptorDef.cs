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
        public int binding;
        public ShaderDescriptorUsage usage;
        public ShaderType type;
        
        public ShaderDescriptorDef(int set, int binding, ShaderDescriptorUsage usage, ShaderType type)
        {
            this.set = set;
            this.binding = binding;
            this.usage = usage;
            this.type = type;
        }

        public override string ToString()
        {
            return $"{set} {binding} {usage} {type}";
        }

        public bool Equals(ShaderDescriptorDef other)
        {
            return set == other.set && binding == other.binding && usage == other.usage && type == other.type;
        }

        public override bool Equals(object? obj)
        {
            return obj is ShaderDescriptorDef other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(set, binding, (int)usage, (int)type);
        }
    }
}