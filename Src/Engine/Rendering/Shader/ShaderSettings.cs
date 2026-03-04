

namespace SpatialSim.Engine.Rendering
{
    public struct ShaderSettings : IEquatable<ShaderSettings>
{
        public ShaderType type;
        public string file;

        public ShaderSettings(ShaderType type, string file)
        {
            this.type = type;
            this.file = file;
        }

        public bool Equals(ShaderSettings other)
        {
            return type == other.type && file == other.file;
        }

        public override bool Equals(object? obj)
        {
            return obj is ShaderSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)type, file);
        }
    }    
}
