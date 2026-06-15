using System.Numerics;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class NormalMapPipeline : Pipeline
    {
        public NormalMapPipeline() : base(DefaultPipelines.BaseNormalMap)
        {
            
        }

        public override void SetDrawData(in CommandBuffer commandBuffer, in MeshRenderer meshRenderer, int uniformBinding)
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
            fragmentShader.AddData(uniformBinding, new Vector3(MathF.Cos((float)AppState.GetSeconds()), 0.01f, 1 + MathF.Sin((float)AppState.GetSeconds())));
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
    }
}