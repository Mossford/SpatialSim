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
            Debug.LogDebug($"Created pipeline with {vertex.settings.file} and {fragment.settings.file}");
        }

        public void UpdateUniforms(in Shader shader, int binding, int frame)
        {
            // TODO this is running two times maybe change?
            int set = RendererSettings.VertexUniformSet;
            if (shader.settings.type == ShaderType.Fragment)
            {
                set = RendererSettings.FragmentUniformSet;
            }
            
            ShaderDescriptorDef def = new ShaderDescriptorDef(set, binding, ShaderDescriptorUsage.Uniform, shader.settings.type);
            pipeline?.UpdateUniforms(shader, binding, frame);
            shader.uniformData[def].Clear();
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