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
            //we can have a empty camera ref so check for it
            if(meshRenderer.camera.id == -1)
                return;
            
            //default pipeline has default behavior
            
            Shader vertexShader = ShaderManager.RetrieveShader(shaders[0]);
            vertexShader.AddData(uniformBinding, meshRenderer.cameraRef.view);
            vertexShader.AddData(uniformBinding, meshRenderer.cameraRef.proj);
            vertexShader.AddData(uniformBinding, meshRenderer.meshRef.transformRef.GetModelMat());
            UpdateUniforms(vertexShader, uniformBinding);
            commandBuffer.BindVertexUniforms(this, uniformBinding);
            Shader fragmentShader = ShaderManager.RetrieveShader(shaders[1]);
            fragmentShader.AddData(uniformBinding, meshRenderer.materialRef.diffuse, true);
            fragmentShader.AddData(uniformBinding, meshRenderer.materialRef.ambient, true);
            fragmentShader.AddData(uniformBinding, new Vector4(meshRenderer.materialRef.specular, meshRenderer.materialRef.specularExp));
            fragmentShader.AddData(uniformBinding, meshRenderer.cameraRef.transformRef.position);
            int colorTextureIndex = TextureManager.RetrieveTextureIndex(meshRenderer.materialRef.textureRef);
            fragmentShader.AddData(uniformBinding, (uint)colorTextureIndex);
            fragmentShader.AddData(uniformBinding, new Vector3(2, 0.5f, -1));
            int normalTextureIndex = TextureManager.RetrieveTextureIndex(meshRenderer.materialRef.normalMapRef);
            fragmentShader.AddData(uniformBinding, (uint)normalTextureIndex);
            UpdateUniforms(fragmentShader, uniformBinding);
            commandBuffer.BindFragmentUniforms(this, uniformBinding);
            commandBuffer.BindSamplers(
                this,
                [TextureManager.RetrieveTexture(meshRenderer.materialRef.textureRef), 
                    TextureManager.RetrieveTexture(meshRenderer.materialRef.normalMapRef)],
                [colorTextureIndex, normalTextureIndex],
                ShaderType.Fragment);
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