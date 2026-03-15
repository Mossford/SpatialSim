namespace SpatialSim.Engine.Rendering
{
    public interface IShaderDevice
    {
        public void Create(ShaderSettings settings, in byte[] code);
        public void Clean();
    }
}