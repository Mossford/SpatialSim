using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public interface IPipelineDevice
    {
        public void Create(in Shader vertex, in Shader fragment);
        public void Bind();
        public void UpdateUniforms(in Shader shader, int frame);
        public void Clean();
    }

    public sealed class Pipeline : IDisposable, IComponent
    {
        public IPipelineDevice? pipeline;

        public EcsComponentType type => EcsComponentType.Pipeline;
        public int id { get; set; } = -1;

        public void Create(in Shader vertex, in Shader fragment)
        {
            pipeline = AppState.appContext.DeviceFactory.CreatePipelineDevice(vertex, fragment);
        }

        public void UpdateUniforms(in Shader shader, int frame)
        {
            pipeline?.UpdateUniforms(shader, frame);
            shader.uniformData.Clear();
        }
        
        public void Bind()
        {
            
        }

        public void Clean()
        {
            pipeline?.Clean();
        }

        public void Dispose()
        {
            Clean();
        }
    }   
}