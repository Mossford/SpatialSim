namespace SpatialSim.Engine.Rendering
{
    public interface IPipelineDevice
    {
        public void CreateRaster(in Shader vertex, in Shader fragment, in PipelineSettings settings);
        public void CreateCompute(in Shader compute, in PipelineSettings settings);
        public void Bind();
        public void UpdateUniforms(in Shader shader, int binding);
        public void Clean();
    }
}