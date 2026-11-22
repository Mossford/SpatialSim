using System.Numerics;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Pipeline : IDisposable
    {
        public IPipelineDevice? pipeline;
        public List<string> shaderNames;
        
        public string name;
        public PipelineSettings settings;
        public int layer;
        public int id;

        public Pipeline(string name)
        {
            this.name = name;
            settings = new PipelineSettings();
            layer = 0;
        }
        
        public void Create(in Shader[] shaders)
        {
            shaderNames = new List<string>();
            pipeline = AppState.appContext.DeviceFactory.CreatePipelineDevice(shaders, settings);
            string names = "";
            for (int i = 0; i < shaders.Length; i++)
            {
                shaderNames.Add(shaders[i].settings.file);
                names += shaders[i].settings.file + ", ";
            }
            Ticks.pipelineCount.created++;
            Debug.LogDebug($"Created pipeline with {names}");
        }

        public void Recreate()
        {
            if (pipeline is null)
            {
                Debug.Warning($"Tried to recreate pipeline {name}, but pipeline was not created in first place");
                return;
            }
            
            Clean();

            Shader[] shaders = new Shader[shaderNames.Count];
            for (int i = 0; i < shaders.Length; i++)
            {
                shaders[i] = ShaderManager.RetrieveShader(shaderNames[i]);
            }
            Create(shaders);
        }

        // TODO maybe move this into separate functions for vertex and fragment
        public virtual void SetData(in CommandBuffer commandBuffer, in MeshRenderer meshRenderer, int uniformBinding)
        {
            
        }
        
        /// <summary>
        /// Function that does not provide any mesh renderer but allows for custom overrides for stuff outside the ecs
        /// </summary>
        public virtual void SetData(in CommandBuffer commandBuffer, int uniformBinding)
        {
            
        }

        public void UpdateUniforms(in Shader shader, int binding)
        {
            int set = shader.settings.type switch
            {
                ShaderType.Fragment => RendererSettings.FragmentUniformSet,
                ShaderType.Vertex => RendererSettings.VertexUniformSet,
                ShaderType.Compute => RendererSettings.ComputeUniformSet,
                _ => RendererSettings.VertexUniformSet
            };

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