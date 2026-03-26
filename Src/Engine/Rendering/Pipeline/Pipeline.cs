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
        
        public string pipelineName;

        public Pipeline(string pipelineName)
        {
            this.pipelineName = pipelineName;
        }
        
        public void Create(in Shader vertex, in Shader fragment)
        {
            shaders = new List<string>();
            pipeline = AppState.appContext.DeviceFactory.CreatePipelineDevice(vertex, fragment);
            shaders.Add(vertex.settings.file);
            shaders.Add(fragment.settings.file);
            Ticks.pipelineCount.created++;
            Debug.LogDebug($"Created pipeline with {vertex.settings.file} and {fragment.settings.file}");
        }

        // TODO maybe move this into separate functions for vertex and fragment
        public virtual void SetDrawData(in CommandBuffer commandBuffer, in MeshRenderer meshRenderer, int binding)
        {
            //default pipeline has default behavior
            
            Shader vertexShader = ShaderManager.RetrieveShader(shaders[0]);
            vertexShader.AddData(binding, meshRenderer.cameraRef.view);
            vertexShader.AddData(binding, meshRenderer.cameraRef.proj);
            vertexShader.AddData(binding, meshRenderer.meshRef.transformRef.GetModelMat());
            UpdateUniforms(vertexShader, binding);
            commandBuffer.BindVertexUniforms(this, binding);
            Shader fragmentShader = ShaderManager.RetrieveShader(shaders[1]);
            fragmentShader.AddData(binding, new Vector4(meshRenderer.materialRef.diffuse, 1.0f));
            fragmentShader.AddData(binding, new Vector4(1.0f));
            fragmentShader.AddData(binding, new Vector4(1.0f));
            fragmentShader.AddData(binding, new Vector4(1.0f));
            UpdateUniforms(fragmentShader, binding);
            commandBuffer.BindFragmentUniforms(this, binding);
            commandBuffer.BindTexture(this, TextureManager.RetrieveTexture(meshRenderer.materialRef.textureRef, pipelineName));
        }

        public void UpdateUniforms(in Shader shader, int binding)
        {
            // TODO this is running two times maybe change?
            int set = RendererSettings.VertexUniformSet;
            if (shader.settings.type == ShaderType.Fragment)
            {
                set = RendererSettings.FragmentUniformSet;
            }
            
            ShaderDescriptorDef def = new ShaderDescriptorDef(set, binding, ShaderDescriptorUsage.Uniform, shader.settings.type);
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