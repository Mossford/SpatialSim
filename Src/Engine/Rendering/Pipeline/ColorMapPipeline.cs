using System.Numerics;

namespace SpatialSim.Engine.Rendering
{
    public class ColorMapPipeline : Pipeline
    {
        public ColorMapPipeline() : base(DefaultPipelines.BaseColor)
        {
            
        }
        
        public override void SetData(in CommandBuffer commandBuffer, in MeshRenderer meshRenderer, int uniformBinding)
        {
            //we can have a empty camera ref so check for it
            if(meshRenderer.camera.id == -1)
                return;
            
            //default pipeline has default behavior
            
            Shader vertexShader = ShaderManager.RetrieveShader(shaderNames[0]);
            vertexShader.AddData(uniformBinding, meshRenderer.cameraRef.view);
            vertexShader.AddData(uniformBinding, meshRenderer.cameraRef.proj);
            vertexShader.AddData(uniformBinding, meshRenderer.transformRef.GetModelMat());
            UpdateUniforms(vertexShader, uniformBinding);
            commandBuffer.BindUniforms(this, uniformBinding, ShaderType.Vertex);
            Shader fragmentShader = ShaderManager.RetrieveShader(shaderNames[1]);
            fragmentShader.AddData(uniformBinding, meshRenderer.materialRef.diffuse, true);
            fragmentShader.AddData(uniformBinding, meshRenderer.materialRef.ambient, true);
            fragmentShader.AddData(uniformBinding, new Vector4(meshRenderer.materialRef.specular, meshRenderer.materialRef.specularExp));
            fragmentShader.AddData(uniformBinding, meshRenderer.cameraRef.transformRef.position);
            int colorTextureIndex = TextureManager.RetrieveTextureIndex(meshRenderer.materialRef.textureRef);
            fragmentShader.AddData(uniformBinding, (uint)colorTextureIndex);
            fragmentShader.AddData(uniformBinding, new Vector3(2, 0.5f, -1), true);
            UpdateUniforms(fragmentShader, uniformBinding);
            commandBuffer.BindUniforms(this, uniformBinding, ShaderType.Fragment);
            commandBuffer.BindSamplers(
                this,
                [TextureManager.RetrieveTexture(colorTextureIndex)],
                [colorTextureIndex],
                ShaderType.Fragment);
        }
    }    
}
