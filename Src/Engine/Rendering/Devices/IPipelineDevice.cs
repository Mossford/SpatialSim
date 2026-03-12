namespace SpatialSim.Engine.Rendering
{
    public interface IPipelineDevice
    {
        public void Create(in Shader vertex, in Shader fragment);
        public void Bind();
        public void UpdateUniforms(in Shader shader, int frame);
        public void Clean();
    }
}