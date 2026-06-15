using System.Numerics;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Pipeline : IDisposable
    {
        public IPipelineDevice? pipeline;
        public List<string> shaders;
        
        public string name;
        public PipelineSettings settings;
        public int layer;

        public Pipeline(string name)
        {
            this.name = name;
            settings = new PipelineSettings();
            layer = 0;
        }
        
        public void Create(in Shader vertex, in Shader fragment)
        {
            shaders = new List<string>();
            pipeline = AppState.appContext.DeviceFactory.CreatePipelineDevice(vertex, fragment, settings);
            shaders.Add(vertex.settings.file);
            shaders.Add(fragment.settings.file);
            Ticks.pipelineCount.created++;
            Debug.LogDebug($"Created pipeline with {vertex.settings.file} and {fragment.settings.file}");
        }

        public void Recreate()
        {
            if (pipeline is null)
            {
                Debug.Warning($"Tried to recreate pipeline {name}, but pipeline was not created in first place");
                return;
            }
            
            Clean();
            
            Create(ShaderManager.RetrieveShader(shaders[0]), ShaderManager.RetrieveShader(shaders[1]));
        }

        // TODO maybe move this into separate functions for vertex and fragment
        public virtual void SetDrawData(in CommandBuffer commandBuffer, in MeshRenderer meshRenderer, int uniformBinding)
        {
            
        }
        
        /// <summary>
        /// Function that does not provide any mesh renderer but allows for custom overrides for stuff outside the ecs
        /// </summary>
        public virtual void SetDrawData(in CommandBuffer commandBuffer, int uniformBinding)
        {
            
        }

        public void UpdateUniforms(in Shader shader, int binding)
        {
            // TODO this is running two times maybe change?
            int set = RendererSettings.VertexUniformSet;
            if (shader.settings.type == ShaderType.Fragment)
            {
                set = RendererSettings.FragmentUniformSet;
            }
            
            ShaderDescriptorDef def = new ShaderDescriptorDef(set, [binding], ShaderDescriptorUsage.Uniform, shader.settings.type);
            pipeline?.UpdateUniforms(shader, binding);
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