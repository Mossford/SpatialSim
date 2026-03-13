using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public sealed class Pipeline : IDisposable, IComponent
    {
        public IPipelineDevice? pipeline;

        public EcsComponentType type => EcsComponentType.Pipeline;
        public int id { get; set; } = -1;

        public void Create(in Shader vertex, in Shader fragment)
        {
            pipeline = AppState.appContext.DeviceFactory.CreatePipelineDevice(vertex, fragment);
            Ticks.pipelineCount.created++;
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
            Ticks.pipelineCount.deleted++;
        }

        public void Dispose()
        {
            Clean();
        }
    }   
}