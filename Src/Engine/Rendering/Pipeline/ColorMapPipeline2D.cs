using System.Numerics;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class ColorMapPipeline2D : Pipeline
    {
        public ColorMapPipeline2D() : base(DefaultPipelines.BaseColor2D)
        {
            
        }
        
        public override void SetDrawData(in CommandBuffer commandBuffer, in MeshRenderer meshRenderer, int uniformBinding)
        {
            //default pipeline has default behavior
            
            Shader vertexShader = ShaderManager.RetrieveShader(shaders[0]);
            vertexShader.AddData(uniformBinding, meshRenderer.transformRef.GetModelMat());
            vertexShader.AddData(uniformBinding, Window.orthoProjection);
            UpdateUniforms(vertexShader, uniformBinding);
            commandBuffer.BindVertexUniforms(this, uniformBinding);
            Shader fragmentShader = ShaderManager.RetrieveShader(shaders[1]);
            fragmentShader.AddData(uniformBinding, meshRenderer.materialRef.diffuse);
            int colorTextureIndex = TextureManager.RetrieveTextureIndex(meshRenderer.materialRef.textureRef);
            fragmentShader.AddData(uniformBinding, (uint)colorTextureIndex);
            UpdateUniforms(fragmentShader, uniformBinding);
            commandBuffer.BindFragmentUniforms(this, uniformBinding);
            commandBuffer.BindSamplers(
                this,
                [TextureManager.RetrieveTexture(colorTextureIndex)],
                [colorTextureIndex],
                ShaderType.Fragment);
        }
    }    
}
